﻿using Loqui;
using Loqui.Generation;
using Mutagen.Bethesda.Binary;
using Mutagen.Bethesda.Internals;
using Noggog;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Mutagen.Bethesda.Generation
{
    public class GenderedTypeBinaryTranslationGeneration : BinaryTranslationGeneration
    {
        public override int? ExpectedLength(ObjectGeneration objGen, TypeGeneration typeGen)
        {
            GenderedType gender = typeGen as GenderedType;

            if (!this.Module.TryGetTypeGeneration(gender.SubTypeGeneration.GetType(), out var subTransl))
            {
                throw new ArgumentException("Unsupported type generator: " + gender.SubTypeGeneration);
            }

            var expected = subTransl.ExpectedLength(objGen, gender.SubTypeGeneration);
            if (expected == null) return null;
            return expected.Value * 2;
        }

        public override void GenerateCopyIn(FileGeneration fg, ObjectGeneration objGen, TypeGeneration typeGen, Accessor readerAccessor, Accessor itemAccessor, Accessor errorMaskAccessor, Accessor translationAccessor)
        {
            GenderedType gender = typeGen as GenderedType;
            var data = typeGen.GetFieldData();

            if (!this.Module.TryGetTypeGeneration(gender.SubTypeGeneration.GetType(), out var subTransl))
            {
                throw new ArgumentException("Unsupported type generator: " + gender.SubTypeGeneration);
            }

            if (data.RecordType.HasValue)
            {
                fg.AppendLine($"{readerAccessor}.Position += {readerAccessor}.{nameof(MutagenBinaryReadStream.MetaData)}.{nameof(GameConstants.SubConstants)}.{nameof(RecordHeaderConstants.HeaderLength)};");
            }
            else if (data.MarkerType.HasValue)
            {
                fg.AppendLine($"{readerAccessor}.Position += {readerAccessor}.{nameof(MutagenBinaryReadStream.MetaData)}.{nameof(GameConstants.SubConstants)}.{nameof(RecordHeaderConstants.HeaderLength)} + contentLength; // Skip marker");
            }

            using (var args = new ArgsWrapper(fg,
                $"{itemAccessor} = {this.Namespace}GenderedItemBinaryTranslation.Parse<{gender.SubTypeGeneration.TypeName(getter: false)}>"))
            {
                args.AddPassArg($"frame");
                if (gender.SubTypeGeneration is FormLinkType
                    || gender.SubTypeGeneration is LoquiType)
                {
                    args.AddPassArg("masterReferences");
                }
                if (gender.MaleMarker.HasValue)
                {
                    args.Add($"maleMarker: {objGen.RecordTypeHeaderName(gender.MaleMarker.Value)}");
                    args.Add($"femaleMarker: {objGen.RecordTypeHeaderName(gender.FemaleMarker.Value)}");
                }
                var subData = gender.SubTypeGeneration.GetFieldData();
                if (subData.RecordType.HasValue
                    && !(gender.SubTypeGeneration is LoquiType))
                {
                    args.Add($"contentMarker: {objGen.RecordTypeHeaderName(subData.RecordType.Value)}");
                }
                LoquiType loqui = gender.SubTypeGeneration as LoquiType;
                if (loqui != null)
                {
                    if (subData?.RecordTypeConverter != null
                        && subData.RecordTypeConverter.FromConversions.Count > 0)
                    {
                        args.Add($"recordTypeConverter: {objGen.RegistrationName}.{typeGen.Name}Converter");
                    }
                }
                if (loqui != null
                    && !loqui.CanStronglyType)
                {
                    args.Add($"transl: {subTransl.GetTranslatorInstance(gender.SubTypeGeneration, getter: false)}.Parse<{loqui.TypeName(getter: false)}>");
                }
                else
                {
                    args.Add($"transl: {subTransl.GetTranslatorInstance(gender.SubTypeGeneration, getter: false)}.Parse");
                    if (gender.ItemHasBeenSet && loqui == null)
                    {
                        args.Add($"skipMarker: false");
                    }
                }
            }
        }

        public override void GenerateCopyInRet(
            FileGeneration fg, 
            ObjectGeneration objGen,
            TypeGeneration targetGen, 
            TypeGeneration typeGen,
            Accessor readerAccessor, 
            AsyncMode asyncMode,
            Accessor retAccessor, 
            Accessor outItemAccessor, 
            Accessor errorMaskAccessor,
            Accessor translationAccessor,
            Accessor mastersAccessor)
        {
            throw new NotImplementedException();
        }

        public override void GenerateWrite(
            FileGeneration fg, 
            ObjectGeneration objGen, 
            TypeGeneration typeGen, 
            Accessor writerAccessor, 
            Accessor itemAccessor, 
            Accessor errorMaskAccessor,
            Accessor translationAccessor,
            Accessor mastersAccessor)
        {
            GenderedType gendered = typeGen as GenderedType;
            var gen = this.Module.GetTypeGeneration(gendered.SubTypeGeneration.GetType());
            var data = typeGen.GetFieldData();
            if (!this.Module.TryGetTypeGeneration(gendered.SubTypeGeneration.GetType(), out var subTransl))
            {
                throw new ArgumentException("Unsupported type generator: " + gendered.SubTypeGeneration);
            }
            var allowDirectWrite = subTransl.AllowDirectWrite(objGen, typeGen);
            bool needsMasters = gendered.SubTypeGeneration is FormLinkType || gendered.SubTypeGeneration is LoquiType;
            var typeName = gendered.SubTypeGeneration.TypeName(getter: true);
            var loqui = gendered.SubTypeGeneration as LoquiType;
            if (loqui != null)
            {
                typeName = loqui.TypeName(getter: true, internalInterface: true);
            }
            using (var args = new ArgsWrapper(fg,
                $"GenderedItemBinaryTranslation.Write"))
            {
                args.Add($"writer: {writerAccessor}");
                args.Add($"item: {itemAccessor}");
                if (data.RecordType.HasValue)
                {
                    args.Add($"recordType: {objGen.RecordTypeHeaderName(data.RecordType.Value)}");
                }
                else if (data.MarkerType.HasValue)
                {
                    args.Add($"markerType: {objGen.RecordTypeHeaderName(data.MarkerType.Value)}");
                }
                if (gendered.MaleMarker.HasValue)
                {
                    args.Add($"maleMarker: {objGen.RecordTypeHeaderName(gendered.MaleMarker.Value)}");
                }
                if (gendered.FemaleMarker.HasValue)
                {
                    args.Add($"femaleMarker: {objGen.RecordTypeHeaderName(gendered.FemaleMarker.Value)}");
                }
                if (gendered.MaleMarker.HasValue
                    && loqui != null)
                {
                    args.Add("markerWrap: false");
                }
                if (needsMasters)
                {
                    args.AddPassArg($"masterReferences");
                }
                if (allowDirectWrite)
                {
                    args.Add($"transl: {subTransl.GetTranslatorInstance(gendered.SubTypeGeneration, getter: true)}.Write{(gendered.SubTypeGeneration.HasBeenSet ? "Nullable" : string.Empty)}");
                }
                else
                {
                    args.Add((gen) =>
                    {
                        var listTranslMask = this.MaskModule.GetMaskModule(gendered.SubTypeGeneration.GetType()).GetTranslationMaskTypeStr(gendered.SubTypeGeneration);
                        gen.AppendLine($"transl: (MutagenWriter subWriter, {typeName}{(gendered.SubTypeGeneration.HasBeenSet ? "?" : null)} subItem{(needsMasters ? $", {nameof(MasterReferenceReader)} m, {nameof(RecordTypeConverter)}? conv" : null)}) =>");
                        using (new BraceWrapper(gen))
                        {
                            subTransl.GenerateWrite(
                                fg: gen,
                                objGen: objGen,
                                typeGen: gendered.SubTypeGeneration,
                                writerAccessor: "subWriter",
                                translationAccessor: "subTranslMask",
                                itemAccessor: new Accessor($"subItem"),
                                errorMaskAccessor: null,
                                mastersAccessor: "m");
                        }
                    });
                }
            }
        }

        public override string GetTranslatorInstance(TypeGeneration typeGen, bool getter)
        {
            throw new NotImplementedException();
        }

        public override void GenerateWrapperFields(
            FileGeneration fg,
            ObjectGeneration objGen,
            TypeGeneration typeGen,
            Accessor dataAccessor,
            int? currentPosition,
            DataType dataType = null)
        {
            var data = typeGen.GetFieldData();
            switch (data.BinaryOverlayFallback)
            {
                case BinaryGenerationType.Normal:
                    break;
                case BinaryGenerationType.DoNothing:
                case BinaryGenerationType.NoGeneration:
                    return;
                case BinaryGenerationType.Custom:
                    this.Module.CustomLogic.GenerateForCustomFlagWrapperFields(
                        fg,
                        objGen,
                        typeGen,
                        dataAccessor,
                        ref currentPosition,
                        dataType);
                    return;
                default:
                    throw new NotImplementedException();
            }

            var gendered = typeGen as GenderedType;
            this.Module.TryGetTypeGeneration(gendered.SubTypeGeneration.GetType(), out var subBin);

            if (typeGen.HasBeenSet
                && !gendered.ItemHasBeenSet)
            {
                var subLen = subBin.ExpectedLength(objGen, gendered.SubTypeGeneration).Value;
                if (typeGen.HasBeenSet)
                {
                    fg.AppendLine($"private int? _{typeGen.Name}Location;");
                }
                fg.AppendLine($"public IGenderedItemGetter<{gendered.SubTypeGeneration.TypeName(getter: true)}>? {typeGen.Name}");
                using (new BraceWrapper(fg))
                {
                    fg.AppendLine("get");
                    using (new BraceWrapper(fg))
                    {
                        fg.AppendLine($"if (!_{typeGen.Name}Location.HasValue) return {typeGen.GetDefault()};");
                        fg.AppendLine($"var data = HeaderTranslation.ExtractSubrecordMemory(_data, _{typeGen.Name}Location.Value, _package.Meta);");
                        using (var args = new ArgsWrapper(fg,
                            $"return new GenderedItem<{gendered.SubTypeGeneration.TypeName(getter: true)}>"))
                        {
                            args.Add($"{subBin.GenerateForTypicalWrapper(objGen, gendered.SubTypeGeneration, "data", "_package")}");
                            args.Add($"{subBin.GenerateForTypicalWrapper(objGen, gendered.SubTypeGeneration, $"data.Slice({subLen})", "_package")}");
                        }
                    }
                }
            }
            else if (!typeGen.HasBeenSet
                && !gendered.ItemHasBeenSet)
            {
                var subLen = subBin.ExpectedLength(objGen, gendered.SubTypeGeneration).Value;
                if (dataType == null)
                {
                    if (typeGen.HasBeenSet)
                    {
                        throw new NotImplementedException();
                        //fg.AppendLine($"public {typeGen.TypeName(getter: true)}? {typeGen.Name} => {dataAccessor}.Length >= {(currentPosition + this.ExpectedLength(objGen, typeGen).Value)} ? {GenerateForTypicalWrapper(objGen, typeGen, $"{dataAccessor}.Span.Slice({currentPosition}, {this.ExpectedLength(objGen, typeGen).Value})", "_package")} : {typeGen.GetDefault()};");
                    }
                    else
                    {
                        fg.AppendLine($"public IGenderedItemGetter<{gendered.SubTypeGeneration.TypeName(getter: true)}> {typeGen.Name}");
                        using (new BraceWrapper(fg))
                        {
                            fg.AppendLine("get");
                            using (new BraceWrapper(fg))
                            {
                                if (typeGen.HasBeenSet)
                                {
                                    fg.AppendLine($"if (!_{typeGen.Name}Location.HasValue) return {typeGen.GetDefault()};");
                                }
                                fg.AppendLine($"var data = {dataAccessor}.Span.Slice({currentPosition}, {subLen * 2});");
                                using (var args = new ArgsWrapper(fg,
                                    $"return new GenderedItem<{gendered.SubTypeGeneration.TypeName(getter: true)}>"))
                                {
                                    args.Add($"{subBin.GenerateForTypicalWrapper(objGen, gendered.SubTypeGeneration, "data", "_package")}");
                                    args.Add($"{subBin.GenerateForTypicalWrapper(objGen, gendered.SubTypeGeneration, $"data.Slice({subLen})", "_package")}");
                                }
                            }
                        }
                    }
                }
                else
                {
                    DataBinaryTranslationGeneration.GenerateWrapperExtraMembers(fg, dataType, objGen, typeGen, currentPosition);
                    fg.AppendLine($"public IGenderedItemGetter<{gendered.SubTypeGeneration.TypeName(getter: true)}>{(typeGen.HasBeenSet ? "?" : null)} {typeGen.Name}");
                    using (new BraceWrapper(fg))
                    {
                        fg.AppendLine("get");
                        using (new BraceWrapper(fg))
                        {
                            fg.AppendLine($"if (!_{typeGen.Name}_IsSet) return new GenderedItem<{gendered.SubTypeGeneration.TypeName(getter: true)}>({gendered.SubTypeGeneration.GetDefault()}, {gendered.SubTypeGeneration.GetDefault()});");
                            fg.AppendLine($"var data = {dataAccessor}.Span.Slice(_{typeGen.Name}Location);");
                            using (var args = new ArgsWrapper(fg,
                                $"return new GenderedItem<{gendered.SubTypeGeneration.TypeName(getter: true)}>"))
                            {
                                args.Add($"{subBin.GenerateForTypicalWrapper(objGen, gendered.SubTypeGeneration, "data", "_package")}");
                                args.Add($"{subBin.GenerateForTypicalWrapper(objGen, gendered.SubTypeGeneration, $"data.Slice({subLen})", "_package")}");
                            }
                        }
                    }
                }
            }
            else
            {
                if (typeGen.HasBeenSet)
                {
                    fg.AppendLine($"private GenderedItemBinaryOverlay<{gendered.SubTypeGeneration.TypeName(getter: true)}>? _{typeGen.Name}Overlay;");
                }
                fg.AppendLine($"public IGenderedItemGetter<{gendered.SubTypeGeneration.TypeName(getter: true)}>? {typeGen.Name} => _{typeGen.Name}Overlay;");
            }
        }

        public override async Task GenerateWrapperRecordTypeParse(
            FileGeneration fg,
            ObjectGeneration objGen,
            TypeGeneration typeGen,
            Accessor locationAccessor,
            Accessor packageAccessor,
            Accessor converterAccessor)
        {
            var gendered = typeGen as GenderedType;
            bool isLoqui = gendered.SubTypeGeneration is LoquiType;
            switch (typeGen.GetFieldData().BinaryOverlayFallback)
            {
                case BinaryGenerationType.Normal:
                    if (gendered.ItemHasBeenSet)
                    {
                        using (var args = new ArgsWrapper(fg,
                            $"_{typeGen.Name}Overlay = GenderedItemBinaryOverlay<{gendered.SubTypeGeneration.TypeName(getter: true)}>.{(isLoqui ? "FactorySkipMarkers" : "Factory")}"))
                        {
                            args.Add("package: _package");
                            args.Add($"male: {objGen.RecordTypeHeaderName(gendered.MaleMarker.Value)}");
                            args.Add($"female: {objGen.RecordTypeHeaderName(gendered.FemaleMarker.Value)}");
                            if (gendered.SubTypeGeneration is LoquiType loqui)
                            {
                                args.Add("bytes: _data.Slice(stream.Position - offset)");
                                args.Add($"creator: (m, p) => {this.Module.BinaryOverlayClassName(loqui)}.{loqui.TargetObjectGeneration.Name}Factory(new {nameof(BinaryMemoryReadStream)}(m), p)");
                            }
                            else
                            {
                                args.AddPassArg("stream");
                                this.Module.TryGetTypeGeneration(gendered.SubTypeGeneration.GetType(), out var subGen);
                                args.Add($"creator: (m, p) => {subGen.GenerateForTypicalWrapper(objGen, typeGen, $"{nameof(HeaderTranslation)}.{nameof(HeaderTranslation.ExtractSubrecordMemory)}(m, p.Meta)", "p")}");
                            }
                        }
                    }
                    else
                    {
                        await base.GenerateWrapperRecordTypeParse(fg, objGen, typeGen, locationAccessor, packageAccessor, converterAccessor);
                    }
                    break;
                default:
                    await base.GenerateWrapperRecordTypeParse(fg, objGen, typeGen, locationAccessor, packageAccessor, converterAccessor);
                    break;
            }
        }
    }
}