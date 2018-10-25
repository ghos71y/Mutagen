﻿using Noggog;
using Noggog.Notifying;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mutagen.Bethesda
{
    public class EDIDSetLink<T> : FormIDSetLink<T>, IEDIDSetLink<T>
       where T : IMajorRecord
    {
        public RecordType EDID { get; private set; } = EDIDLink<T>.UNLINKED;

        public EDIDSetLink()
            : base()
        {
        }

        public EDIDSetLink(RecordType unlinkedEDID)
            : this()
        {
            this.EDID = unlinkedEDID;
        }

        public EDIDSetLink(FormID unlinkedForm)
            : base(unlinkedForm)
        {
        }

        private void HandleItemChange(Change<T> change)
        {
            this.EDID = EDIDLink<T>.UNLINKED;
            change.Old?.EditorID_Property.Unsubscribe(this);
            change.New?.EditorID_Property.Subscribe(this, UpdateUnlinked);
        }

        private void UpdateUnlinked(Change<string> change)
        {
            this.EDID = new RecordType(change.New);
        }

        public void Set(IEDIDLink<T> link, NotifyingFireParameters cmds = null)
        {
            if (link.Linked)
            {
                this.Set(link.Item, cmds);
            }
            else
            {
                this.EDID = link.EDID;
            }
        }

        public override void Set(T value, bool hasBeenSet, NotifyingFireParameters cmds = null)
        {
            HandleItemChange(new Change<T>(this.Item, value));
            base.Set(value, hasBeenSet, cmds);
        }

        public void SetIfSucceeded(TryGet<RecordType> item)
        {
            if (item.Failed) return;
            Set(item.Value);
        }

        public void Set(RecordType item)
        {
            this.EDID = item;
            this.HasBeenSet = true;
        }

        public void SetIfSuccessful(TryGet<string> item)
        {
            if (!item.Succeeded) return;
            Set(item.Value);
        }

        public void Set(string item)
        {
            this.EDID = new RecordType(item);
            this.HasBeenSet = true;
        }

        public override bool Link<M>(ModList<M> modList, M sourceMod, NotifyingFireParameters cmds = null)
        {
            if (this.UnlinkedForm.HasValue)
            {
                return base.Link(modList, sourceMod, cmds);
            }
            return EDIDLink<T>.TryLink(this, modList, sourceMod, cmds);
        }
    }
}
