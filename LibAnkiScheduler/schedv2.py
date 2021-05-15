# Copyright: Ankitects Pty Ltd and contributors
# License: GNU AGPL, version 3 or later; http://www.gnu.org/licenses/agpl.html

# https://github.com/ankitects/anki/blob/587686656503cf4fbfa8d2603da4e2fbc1e92e5d/pylib/anki/schedv2.py

import clr
clr.AddReference("LibAnkiCards")

from LibAnkiCards import NewCardOrdering, CardQueueType, CardLearnType

from heapq import *

class Scheduler:
    name = "std2"

    def __init__(self, cs):
        self.cs = cs
        self.queueLimit = 50
        self.reportLimit = 1000
        self.dynReportLimit = 99999
        self.reps = 0
        self.today = None
        self._haveQueues = False
        self._lrnCutoff = 0
        self._updateCutoff()

    def reset(self):
        self._updateCutoff()
        self._resetLrn()
        self._resetRev()
        self._resetNew()
        self._haveQueues = True

    # Daily cutoff
    ##########################################################################

    def _updateCutoff(self):
        oldToday = self.today

        self.today = self.cs.DaysSinceCreation()
        self.dayCutoff = self.cs.DayCutoff()

        # update all daily counts, but don't save decks to prevent needless
        # conflicts. we'll save on card answer instead
        self.cs.ResetToday(self.today)

        # TODO unbury if the day has rolled over
        #unburied = self.cs.Collection.Configuration.LastUnburied
        #if unburied < self.today:
            #self.unburyCards()
            #self.cs.Collection.Configuration.LastUnburied = self.today
 
    def _checkDay(self):
        # check if the day has rolled over
        if self.cs.Time > self.dayCutoff:
            self.reset()

    # Fetching the next card
    ##########################################################################

    def getCard(self):
        """Pop the next card from the queue. None if finished."""
        self._checkDay()
        if not self._haveQueues:
            self.reset()
        card = self._getCard()
        if card:
            # TODO
            # if not self._burySiblingsOnAnswer:
            #    self._burySiblings(card)
            self.cs.CardStartTimer(card)
            return card
        return None

    def _getCard(self):
        """Return the next due card, or None."""
        # learning card due?
        c = self._getLrnCard()
        if c:
            return c

        # new first, or time for one?
        if self._timeForNewCard():
            c = self._getNewCard()
            if c:
                return c

        # day learning first and card due?
        dayLearnFirst = self.cs.Collection.Configuration.DayLearnFirst
        if dayLearnFirst:
            c = self._getLrnDayCard()
            if c:
                return c

        # card due for review?
        c = self._getRevCard()
        if c:
            return c

        # day learning card due?
        if not dayLearnFirst:
            c = self._getLrnDayCard()
            if c:
                return c

        # new cards left?
        c = self._getNewCard()
        if c:
            return c

        # collapse or finish
        return self._getLrnCard(collapse=True)

    def answerCard(self, card, ease):
        assert 1 <= ease <= 4
        assert 0 <= int(card.Queue) <= 4
        
        # TODO: undo
        # self.col.markReview(card)

        # TODO
        # if self._burySiblingsOnAnswer:
        #    self._burySiblings(card)

        self._answerCard(card, ease)

        self.cs.UpdateStats(card, "time", self.cs.CardTimeTaken(card))
        self.cs.FlushCard(card)

    def _answerCard(self, card, ease):
        # TODO
        # if self._previewingCard(card):
        #    self._answerCardPreview(card, ease)
        #    return

        card.Reps += 1

        if card.Queue == CardQueueType.New:
            # came from the new queue, move to learning
            card.Queue = CardQueueType.LearnRelearn
            card.Type = CardLearnType.Learn
            # init reps to graduation
            card.Left = self.cs.StartingLeft(card, self.dayCutoff)
            # update daily limit
            self.cs.UpdateStats(card, "new")

        if card.Queue == CardQueueType.LearnRelearn or card.Queue == CardQueueType.DayLearnRelearn:
            self._answerLrnCard(card, ease)
        elif card.Queue == CardQueueType.Review:
            self._answerRevCard(card, ease)
            # update daily limit
            self.cs.UpdateStats(card, "rev")
        else:
            assert 0

        # once a card has been answered once, the original due date
        # no longer applies
        if card.OriginalDue:
            card.OriginalDue = 0

    # Learning queues
    ##########################################################################

    def _resetLrn(self):
        self._updateLrnCutoff(force=True)
        self._resetLrnCount()
        self._lrnQueue = []
        self._lrnDayQueue = []
        self._lrnDids = self.cs.MakeCopyOfActives()

    def _maybeResetLrn(self, force):
        if self._updateLrnCutoff(force):
            self._resetLrn()

    def _updateLrnCutoff(self, force):
        nextCutoff = self.cs.Time + self.cs.Collection.Configuration.CollapseTime
        if nextCutoff - self._lrnCutoff > 60 or force:
            self._lrnCutoff = nextCutoff
            return True
        return False

    def _resetLrnCount(self):
        self.lrnCount = self.cs.LearnCount(self._lrnCutoff, self.today)

    def _getLrnCard(self, collapse = False):
        self._maybeResetLrn(force=collapse and self.lrnCount == 0)
        if self._fillLrn():
            cutoff = self.cs.Time
            if collapse:
                cutoff += self.cs.Collection.Configuration.CollapseTime
            if self._lrnQueue[0][0] < cutoff:
                id = heappop(self._lrnQueue)[1]
                card = self.cs.GetCard(id)
                self.lrnCount -= 1
                return card
        return None

    def _fillLrn(self):
        if not self.lrnCount:
            return False
        if self._lrnQueue:
            return True
        cutoff = self.cs.Time + self.cs.Collection.Configuration.CollapseTime
        self._lrnQueue = self.cs.QueryLearnQueue(cutoff, self.reportLimit)

        return self._lrnQueue

    def _getLrnDayCard(self):
        if self._fillLrnDay():
            self.lrnCount -= 1
            return self.cs.GetCard(self._lrnDayQueue.pop())
        return None

    def _fillLrnDay(self):
        if not self.lrnCount:
            return False
        if self._lrnDayQueue:
            return True
        while self._lrnDids:
            did = self._lrnDids[0]
            # fill the queue with the current did
            self._lrnDayQueue = self.cs.QueryLearnDayQueue(did, self.today, self.queueLimit)
            if self._lrnDayQueue:
                # is the current did empty?
                if len(self._lrnDayQueue) < self.queueLimit:
                    self._lrnDids.pop(0)
                return True
            # nothing left in the deck; move to next
            self._lrnDids.pop(0)
        # shouldn't reach here
        return False

    def _answerLrnCard(self, card, ease):
        # TODO conf = self._lrnConf(card)
        if card.Type == CardLearnType.Review or card.Type == CardLearnType.Relearn:
            type = CardLearnType.Review
        else:
            type = CardLearnType.New
        # lrnCount was decremented once when card was fetched
        lastLeft = card.Left

        leaving = False

        # immediate graduate?
        if ease == 4:
            self._rescheduleAsRev(card, True)
            leaving = True
        # next step?
        elif ease == 3:
            # graduation time?
            if (card.Left % 1000) - 1 <= 0:
                self._rescheduleAsRev(card, False)
                leaving = True
            else:
                self._moveToNextStep(card)
        elif ease == 2:
            self._repeatStep(card)
        else:
            # back to first step
            self._moveToFirstStep(card)

        self.cs.LogLearn(card, ease, leaving, type, lastLeft)

    def _rescheduleAsRev(self, card, early):
        if card.Type == CardLearnType.Review or card.Type == CardLearnType.Relearn:
            self._rescheduleGraduatingLapse(card, early)
        else:
            self._rescheduleNew(card, early)
        
        # TODO
        # if we were dynamic, graduating means moving back to the old deck
        # if card.odid:
        #    self._removeFromFiltered(card)

    def _rescheduleGraduatingLapse(self, card, early = False):
        if early:
            card.Ivl += 1
        card.Due = self.today + card.Ivl
        card.Queue = CardQueueType.Review
        card.Type = CardLearnType.Review

    def _rescheduleNew(self, card, early):
        "Reschedule a new card that's graduated for the first time."
        card.Ivl = self._graduatingIvl(card, early)
        card.Due = self.today + card.Ivl
        card.Factor = card.GetDeck(self.cs.Collection).GetConfiguration(self.cs.Collection).InintialFactor
        card.Queue = CardQueueType.Review
        card.Type = CardLearnType.Review

    def _graduatingIvl(self, card, early, fuzz = True):
        if card.Type == CardLearnType.Review or card.Type == CardLearnType.Relearn:
            bonus = early and 1 or 0
            return card.Ivl + bonus
        if not early:
            # graduate
            ideal = card.GetDeck(self.cs.Collection).GetConfiguration(self.cs.Collection).Ints.Graduate
        else:
            # early remove
            ideal = card.GetDeck(self.cs.Collection).GetConfiguration(self.cs.Collection).Ints.EarlyRemove
        # TODO
        # if fuzz:
        #    ideal = self._fuzzedIvl(ideal)
        return ideal

    def _moveToFirstStep(self, card):
        card.Left = self.cs.StartingLeft(card, self.dayCutoff)

        # relearning card?
        if card.Type == CardLearnType.Relearn:
            self._updateRevIvlOnFail(card)

        return self._rescheduleLrnCard(card)

    def _updateRevIvlOnFail(self, card):
        card.LastIvl = card.Ivl
        card.Ivl = self._lapseIvl(card)

    def _lapseIvl(self, card):
        return max(1, card.GetDeck(self.cs.Collection).GetConfiguration(self.cs.Collection).MinInt, int(card.Ivl * card.GetDeck(self.cs.Collection).GetConfiguration(self.cs.Collection).Mult))

    def _moveToNextStep(self, card):
        # decrement real left count and recalculate left today
        left = (card.Left % 1000) - 1
        card.Left = self.cs.LeftToday(card, self.dayCutoff, left) * 1000 + left

        self._rescheduleLrnCard(card)

    def _repeatStep(self, card):
        delay = self.cs.DelayForRepeatingGrade(card, card.Left)
        self._rescheduleLrnCard(card, delay=delay)

    def _rescheduleLrnCard(self, card, delay = None):
        # normal delay for the current step?
        if delay is None:
            delay = self.cs.DelayForGrade(card, card.Left)

        card.Due = self.cs.Time + delay
        # due today?
        if card.Due < self.dayCutoff:
            # TODO
            # add some randomness, up to 5 minutes or 25%
            # maxExtra = min(300, int(delay * 0.25))
            # fuzz = random.randrange(0, maxExtra)
            # card.due = min(self.dayCutoff - 1, card.due + fuzz)
            card.Queue = CardQueueType.LearnRelearn
            if card.Due < (self.cs.Time + self.cs.Collection.Configuration.CollapseTime):
                self.lrnCount += 1
                # if the queue is not empty and there's nothing else to do, make
                # sure we don't put it at the head of the queue and end up showing
                # it twice in a row
                if self._lrnQueue and not self.revCount and not self.newCount:
                    smallestDue = self._lrnQueue[0][0]
                    card.Due = max(card.Due, smallestDue + 1)
                heappush(self._lrnQueue, (card.Due, card.Id))
        else:
            # the card is due in one or more days, so we need to use the
            # day learn queue
            ahead = ((card.Due - self.dayCutoff) // 86400) + 1
            card.Due = self.today + ahead
            card.Queue = CardQueueType.DayLearnRelearn
        return delay

    # Reviews
    ##########################################################################

    def _resetRev(self):
        self._resetRevCount()
        self._revQueue = []

    def _resetRevCount(self):
        lim = self._currentRevLimit()
        self.revCount = self.cs.RevCount(self.today, lim)

    def _currentRevLimit(self):
        return self.cs.DeckRevLimitSingle(self.cs.SelectedDeck)

    def _getRevCard(self):
        if self._fillRev():
            self.revCount -= 1
            return self.cs.GetCard(self._revQueue.pop())
        return None

    def _fillRev(self):
        if self._revQueue:
            return True
        if not self.revCount:
            return False

        lim = min(self.queueLimit, self._currentRevLimit())
        if lim:
            self._revQueue = self.cs.QueryReviewQueue(self.today, lim)

            if self._revQueue:
                return True

        if self.revCount:
            # if we didn't get a card but the count is non-zero,
            # we need to check again for any cards that were
            # removed from the queue but not buried
            self._resetRev()
            return self._fillRev()

    def _answerRevCard(self, card, ease):
        delay = 0
        # TODO
        # early = card.odid and (card.odue > self.today)
        # type = early and 3 or 1
        type = CardLearnType.Learn

        if ease == 1:
            delay = self._rescheduleLapse(card)
        else:
            self._rescheduleRev(card, ease)

        self.cs.LogReview(card, ease, delay, type)

    def _rescheduleLapse(self, card):
        # conf = self._lapseConf(card)

        card.Lapses += 1
        card.Factor = max(1300, card.Factor - 200)

        # TODO
        # suspended = self._checkLeech(card, conf) and card.queue == -1

        card.Type = CardLearnType.Relearn
        delay = self._moveToFirstStep(card)

        return delay

    def _rescheduleRev(self, card, ease):
        # update interval
        card.LastIvl = card.Ivl

        self._updateRevIvl(card, ease)

        # then the rest
        card.Factor = max(1300, card.Factor + [-150, 0, 150][ease - 2])
        card.Due = self.today + card.Ivl

        # card leaves filtered deck
        # self._removeFromFiltered(card)

    def _updateRevIvl(self, card, ease):
        card.Ivl = self._nextRevIvl(card, ease)

    def _nextRevIvl(self, card, ease):
        "Next review interval for CARD, given EASE."
        delay = max(0, self.today - card.Due)
        
        fct = card.Factor / 1000
        hardFactor = card.GetDeck(self.cs.Collection).GetConfiguration(self.cs.Collection).HardFactor
        if hardFactor > 1:
            hardMin = card.Ivl
        else:
            hardMin = 0
        ivl2 = self._constrainedIvl(card.Ivl * hardFactor, hardMin)
        if ease == 2:
            return ivl2

        ivl3 = self._constrainedIvl((card.Ivl + delay // 2) * fct, ivl2)
        if ease == 3:
            return ivl3

        ivl4 = self._constrainedIvl(
            (card.Ivl + delay) * fct * card.GetDeck(self.cs.Collection).GetConfiguration(self.cs.Collection).Ease4, ivl3
        )
        return ivl4

    def _constrainedIvl(self, ivl, prev):
        ivl = max(ivl, prev + 1, 1)
        ivl = min(ivl, 36500) # deckConf -> maxIvl
        return int(ivl)

    # New cards
    ##########################################################################

    def _resetNew(self):
        self._resetNewCount()
        self._newDids = self.cs.MakeCopyOfActives()
        self._newQueue = []
        self._updateNewCardRatio()

    def _resetNewCount(self):
        self.newCount = self.cs.NewCount()

    def _updateNewCardRatio(self):
        if self.cs.Collection.Configuration.NewCardOrdering == NewCardOrdering.Distribute:
            if self.newCount:
                self.newCardModulus = (self.newCount + self.revCount) // self.newCount
                # if there are cards to review, ensure modulo >= 2
                if self.revCount:
                    self.newCardModulus = max(2, self.newCardModulus)
                return
        self.newCardModulus = 0

    def _timeForNewCard(self):
        "True if it's time to display a new card when distributing."
        if not self.newCount:
            return False
        if self.cs.Collection.Configuration.NewCardOrdering == NewCardOrdering.Last:
            return False
        elif self.cs.Collection.Configuration.NewCardOrdering == NewCardOrdering.First:
            return True
        elif self.newCardModulus:
            return self.reps and self.reps % self.newCardModulus == 0
        else:
            # shouldn't reach
            return False

    def _getNewCard(self):
        if self._fillNew():
            self.newCount -= 1
            return self.cs.GetCard(self._newQueue.pop())
        return None

    def _fillNew(self) :
        if self._newQueue:
            return True
        if not self.newCount:
            return False
        while self._newDids:
            did = self._newDids[0]
            lim = min(self.queueLimit, self.cs.DeckNewLimit(did))
            if lim:
                # fill the queue with the current did
                self._newQueue = self.cs.QueryNewQueue(did, lim)
                if self._newQueue:
                    return True
            # nothing left in the deck; move to next
            self._newDids.pop(0)
        if self.newCount:
            # if we didn't get a card but the count is non-zero,
            # we need to check again for any cards that were
            # removed from the queue but not buried
            self._resetNew()
            return self._fillNew()