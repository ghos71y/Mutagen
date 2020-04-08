﻿using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Loqui;
using Loqui.Internal;
using Mutagen.Bethesda.Binary;
using Mutagen.Bethesda.Internals;
using Mutagen.Bethesda.Oblivion.Internals;
using Noggog;

namespace Mutagen.Bethesda.Oblivion
{
    namespace Internals
    {
        public partial class DialogTopicCommon
        {
            partial void PostDuplicate(DialogTopic obj, DialogTopic rhs, Func<FormKey> getNextFormKey, IList<(IMajorRecordCommon Record, FormKey OriginalFormKey)>? duplicatedRecords)
            {
                obj.Items.SetTo(rhs.Items.Select((dia) => (DialogItem)dia.Duplicate(getNextFormKey, duplicatedRecords)));
            }
        }

        public partial class DialogTopicBinaryCreateTranslation
        {
            static partial void CustomBinaryEndImport(MutagenFrame frame, IDialogTopicInternal obj)
            {
                if (frame.Reader.Complete) return;
                GroupHeader groupMeta = frame.GetGroup();
                if (!groupMeta.IsGroup) return;
                if (groupMeta.GroupType == (int)GroupTypeEnum.TopicChildren)
                {
                    obj.Timestamp = groupMeta.LastModifiedSpan.ToArray();
                    if (FormKey.Factory(frame.MasterReferences!, BinaryPrimitives.ReadUInt32LittleEndian(groupMeta.ContainedRecordTypeSpan)) != obj.FormKey)
                    {
                        throw new ArgumentException("Dialog children group did not match the FormID of the parent.");
                    }
                }
                else
                {
                    return;
                }
                frame.Reader.Position += groupMeta.HeaderLength;
                obj.Items.SetTo(Mutagen.Bethesda.Binary.ListBinaryTranslation<DialogItem>.Instance.Parse(
                    frame: frame.SpawnWithLength(groupMeta.ContentLength),
                    transl: (MutagenFrame r, RecordType header, out DialogItem listItem) =>
                    {
                        return LoquiBinaryTranslation<DialogItem>.Instance.Parse(
                            frame: r,
                            item: out listItem);
                    }));
            }
        }

        public partial class DialogTopicBinaryWriteTranslation
        {
            static partial void CustomBinaryEndExport(MutagenWriter writer, IDialogTopicGetter obj)
            {
                if (obj.Items == null || obj.Items.Count == 0) return;
                using (HeaderExport.ExportHeader(writer, Group_Registration.GRUP_HEADER, ObjectType.Group))
                {
                    FormKeyBinaryTranslation.Instance.Write(
                        writer,
                        obj.FormKey);
                    writer.Write((int)GroupTypeEnum.TopicChildren);
                    writer.Write(obj.Timestamp);
                    Mutagen.Bethesda.Binary.ListBinaryTranslation<IDialogItemGetter>.Instance.Write(
                        writer: writer,
                        items: obj.Items,
                        transl: (MutagenWriter subWriter, IDialogItemGetter subItem) =>
                        {
                            subItem.WriteToBinary(subWriter);
                        });
                }
            }
        }

        public partial class DialogTopicBinaryOverlay
        {
            private ReadOnlyMemorySlice<byte>? _grupData;

            public ReadOnlyMemorySlice<byte> Timestamp => _grupData != null ? _package.Meta.Group(_grupData.Value).LastModifiedSpan.ToArray() : default(ReadOnlyMemorySlice<byte>);

            public IReadOnlyList<IDialogItemGetter> Items { get; private set; } = ListExt.Empty<IDialogItemGetter>();

            partial void CustomEnd(IBinaryReadStream stream, int finalPos, int offset)
            {
                if (stream.Complete) return;
                var startPos = stream.Position;
                var groupMeta = this._package.Meta.GetGroup(stream);
                if (!groupMeta.IsGroup) return;
                if (groupMeta.GroupType != (int)GroupTypeEnum.TopicChildren) return;
                this._grupData = stream.ReadMemory(checked((int)groupMeta.TotalLength));
                var formKey = FormKey.Factory(_package.MasterReferences, BinaryPrimitives.ReadUInt32LittleEndian(groupMeta.ContainedRecordTypeSpan));
                if (formKey != this.FormKey)
                {
                    throw new ArgumentException("Dialog children group did not match the FormID of the parent.");
                }
                var contentSpan = this._grupData.Value.Slice(_package.Meta.GroupConstants.HeaderLength);
                this.Items = BinaryOverlaySetList<IDialogItemGetter>.FactoryByArray(
                    contentSpan,
                    _package,
                    getter: (s, p) => DialogItemBinaryOverlay.DialogItemFactory(new BinaryMemoryReadStream(s), p),
                    locs: ParseRecordLocations(
                        stream: new BinaryMemoryReadStream(contentSpan),
                        finalPos: contentSpan.Length,
                        trigger: DialogItem_Registration.TriggeringRecordType,
                        constants: GameConstants.Oblivion.MajorConstants,
                        skipHeader: false));
            }
        }
    }
}
