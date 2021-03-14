# Copyright: Ankitects Pty Ltd and contributors
# License: GNU AGPL, version 3 or later; http://www.gnu.org/licenses/agpl.html

# https://github.com/ankitects/anki/blob/587686656503cf4fbfa8d2603da4e2fbc1e92e5d/pylib/anki/schedv2.py

import clr
clr.AddReference("LibAnkiCards")

from LibAnkiCards import NewCardOrdering

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

        # card due for review?
        c = self._getRevCard()
        if c:
            return c

        # new cards left?
        c = self._getNewCard()
        if c:
            return c

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