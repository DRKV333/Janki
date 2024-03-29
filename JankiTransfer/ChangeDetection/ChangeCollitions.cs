﻿using JankiTransfer.DTO;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace JankiTransfer.ChangeDetection
{
    public class ChangeCollitions
    {
        public enum ChangeType
        {
            Added,
            Changed,
            Removed
        }

        public class Change
        {
            public string Type { get; set; }
            public string Name { get; set; }
            public ChangeType ChangeType { get; set; }
            public string ChangeDescrition { get; set; }

            public Action Remove { get; set; }
        }

        public class Collition
        {
            public Change Local { get; set; }
            public Change Remote { get; set; }
        }

        private class ChangeGroup<T>
        {
            public Func<ChangeData, IList<T>> Added { get; set; }
            public Func<ChangeData, IList<T>> Changed { get; set; }
            public Func<ChangeData, IList<Guid>> Removed { get; set; }
            public Func<T, Guid> Id { get; set; }
            public string Type { get; set; }
            public Func<T, string> Description { get; set; }
            public Func<T, string> Name { get; set; }
        }

        private readonly ChangeData local;
        private readonly ChangeData remote;

        public ObservableCollection<Change> LocalChanges { get; } = new ObservableCollection<Change>();
        public ObservableCollection<Change> RemoteChanges { get; } = new ObservableCollection<Change>();
        public ObservableCollection<Collition> Collitions { get; } = new ObservableCollection<Collition>();

        public ChangeCollitions(ChangeData local, ChangeData remote)
        {
            string[] emtyStringArr = new string[0];

            this.local = local;
            this.remote = remote;

            ChangeData remoteCopy = new ChangeData()
            {
                CardsAdded = remote.CardsAdded.ToList(),
                CardsChanged = remote.CardsChanged.ToList(),
                CardsRemoved = remote.CardsRemoved.ToList(),
                CardTypesAdded = remote.CardTypesAdded.ToList(),
                CardTypesChanged = remote.CardTypesChanged.ToList(),
                CardTypesRemoved = remote.CardTypesRemoved.ToList(),
                DecksAdded = remote.DecksAdded.ToList(),
                DecksRemoved = remote.DecksRemoved.ToList()
            };

            ChangeGroup<CardTypeData> cardType = new ChangeGroup<CardTypeData>()
            {
                Added = x => x.CardTypesAdded,
                Removed = x => x.CardTypesRemoved,
                Changed = x => x.CardTypesChanged,
                Id = x => x.Id,
                Type = "Card Type",
                Name = x => x.Name,
                Description = x => string.Join("\n",
                    string.Join("\n", x.VariantsAdded?.Select(y => $"+ {y.Name}") ?? emtyStringArr),
                    string.Join("\n", x.VariantsChanged?.Select(y => $"{y.Name} -> {y.FrontFormat ?? ""}{y.BackFormat ?? ""}") ?? emtyStringArr),
                    string.Join("\n", x.VariantsRemoved?.Select(y => $"- {y}") ?? emtyStringArr))
            };

            MakeChanges(cardType, remoteCopy);

            ChangeGroup<DeckData> deck = new ChangeGroup<DeckData>()
            {
                Added = x => x.DecksAdded,
                Changed = x => new List<DeckData>(),
                Removed = x => x.DecksRemoved,
                Id = x => x.Id,
                Name = x => x.Name,
                Type = "Deck",
                Description = x => ""
            };

            MakeChanges(deck, remoteCopy);

            ChangeGroup<CardData> card = new ChangeGroup<CardData>()
            {
                Added = x => x.CardsAdded,
                Changed = x => x.CardsChanged,
                Removed = x => x.CardsRemoved,
                Id = x => x.Id,
                Type = "Card",
                Name = x => "",
                Description = x => string.Join("\n",
                    string.Join("\n", x.FieldsAdded?.Select(y => $"+ {y.CardFieldTypeId}") ?? emtyStringArr),
                    string.Join("\n", x.FieldsChanged?.Select(y => $"{y.CardFieldTypeId} -> {y.Content}") ?? emtyStringArr),
                    string.Join("\n", x.FieldsRemoved?.Select(y => $"- {y}") ?? emtyStringArr)
                )
            };

            MakeChanges(card, remoteCopy);
        }

        private void MakeChanges<T>(ChangeGroup<T> group, ChangeData remoteCopy)
        {
            MakeChangesForOneList(group.Added(local), group, remoteCopy, true, LocalChanges, RemoteChanges);
            MakeChangesForOneList(group.Changed(local), group, remoteCopy, true, LocalChanges, RemoteChanges);
            MakeChangesForRemoved(group.Removed(local), group, remoteCopy, true, LocalChanges, RemoteChanges);

            MakeChangesForOneList(group.Added(remoteCopy), group, remoteCopy, false, RemoteChanges, LocalChanges);
            MakeChangesForOneList(group.Changed(remoteCopy), group, remoteCopy, false, RemoteChanges, LocalChanges);
            MakeChangesForRemoved(group.Removed(remoteCopy), group, remoteCopy, false, RemoteChanges, LocalChanges);
        }

        private void MakeChangesForOneList<T>(IList<T> theList, ChangeGroup<T> group, ChangeData remoteCopy, bool checkRemote, IList<Change> whereList, IList<Change> otherList)
        {
            foreach (var item in theList)
            {
                Change change = MakeSingleChange(item, group, ChangeType.Added, null);

                if (checkRemote)
                {
                    Change other = AnyOtherChange(change, group.Id(item), group, remoteCopy, remote);
                    if (other != null)
                    {
                        change.Remove = () => { theList.Remove(item); otherList.Add(other); };
                        Collitions.Add(new Collition() { Local = change, Remote = other });
                        continue;
                    }
                }

                whereList.Add(change);
            }
        }

        private void MakeChangesForRemoved<T>(IList<Guid> theList, ChangeGroup<T> group, ChangeData remoteCopy, bool checkRemote, IList<Change> whereList, IList<Change> otherList)
        {
            foreach (var item in theList)
            {
                Change change = new Change()
                {
                    ChangeDescrition = "",
                    ChangeType = ChangeType.Removed,
                    Name = item.ToString(),
                    Type = group.Type
                };

                if (checkRemote)
                {
                    Change other = AnyOtherChange(change, item, group, remoteCopy, remote);
                    if (other != null)
                    {
                        change.Remove = () => { theList.Remove(item); otherList.Add(other); };
                        Collitions.Add(new Collition() { Local = change, Remote = other });
                        continue;
                    }
                }

                whereList.Add(change);
            }
        }

        private Change MakeSingleChange<T>(T thing, ChangeGroup<T> group, ChangeType type, Action remove) => new Change()
        {
            Type = group.Type,
            Name = group.Name(thing),
            ChangeDescrition = group.Description(thing),
            ChangeType = type,
            Remove = remove
        };

        private Change AnyOtherChange<T>(Change change, Guid id, ChangeGroup<T> group, ChangeData data, ChangeData orig)
        {
            T added = group.Added(data).FirstOrDefault(x => group.Id(x) == id);
            if (added != null)
            {
                group.Added(data).Remove(added);
                return MakeSingleChange(added, group, ChangeType.Added, () => { group.Added(orig).Remove(added); LocalChanges.Add(change); });
            }

            T changed = group.Changed(data).FirstOrDefault(x => group.Id(x) == id);
            if (changed != null)
            {
                group.Changed(data).Remove(changed);
                return MakeSingleChange(changed, group, ChangeType.Changed, () => { group.Changed(orig).Remove(changed); LocalChanges.Add(change); });
            }

            if (group.Removed(data).Contains(id))
            {
                group.Removed(data).Remove(id);
                return new Change()
                {
                    ChangeDescrition = "",
                    ChangeType = ChangeType.Removed,
                    Name = change.Name,
                    Type = group.Type,
                    Remove = () => { group.Removed(orig).Remove(id); LocalChanges.Add(change); }
                };
            }

            return null;
        }
    }
}