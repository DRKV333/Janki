# Copyright: Ankitects Pty Ltd and contributors
# License: GNU AGPL, version 3 or later; http://www.gnu.org/licenses/agpl.html

# https://github.com/ankitects/anki/blob/587686656503cf4fbfa8d2603da4e2fbc1e92e5d/pylib/anki/schedv2.py

import clr
clr.AddReference("LibAnkiCards")

from LibAnkiCards import NewCardOrdering

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
            self.col.log(card)
            if not self._burySiblingsOnAnswer:
                self._burySiblings(card)
            card.startTimer()
            return card
        return None

    # Learning queues
    ##########################################################################

    def _resetLrn(self):
        self._updateLrnCutoff(force=True)
        self._resetLrnCount()
        self._lrnQueue = []
        self._lrnDayQueue = []
        # TODO self._lrnDids = self.col.decks.active()[:]

    def _updateLrnCutoff(self, force):
        nextCutoff = self.cs.Time + self.cs.Collection.Configuration.CollapseTime
        if nextCutoff - self._lrnCutoff > 60 or force:
            self._lrnCutoff = nextCutoff
            return True
        return False

    def _resetLrnCount(self):
        self.lrnCount = self.cs.LearnCount(self._lrnCutoff, self.today)

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

    # New cards
    ##########################################################################

    def _resetNew(self):
        self._resetNewCount()
        # TODO self._newDids = self.col.decks.active()[:]
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