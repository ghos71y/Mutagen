﻿using Loqui;
using Loqui.Generation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mutagen.Bethesda.Generation
{
    public class ObservableModModule : GenerationModule
    {
        public override async Task MiscellaneousGenerationActions(ObjectGeneration obj)
        {
            if (obj.GetObjectType() != ObjectType.Mod) return;
            FileGeneration fg = new FileGeneration();
            AddUsings(fg);
            fg.AppendLine($"namespace {obj.Namespace}");
            using (new BraceWrapper(fg))
            {
                fg.AppendLine($"public partial class {obj.Name}_Observable : ObservableModBase");
                using (new BraceWrapper(fg))
                {
                    await GenerateMembers(obj, fg);
                    GenerateCtor(obj, fg);
                    await GenerateFromPath(obj, fg);
                }
            }

            var fileName = Path.Combine(obj.TargetDir.FullName, $"{obj.Name}_Observable_{ObjectGeneration.AUTOGENERATED}.cs");
            fg.Generate(fileName);
            obj.ProtoGen.GeneratedFiles[fileName] = ProjItemType.Compile;
        }

        private static void GenerateCtor(ObjectGeneration obj, FileGeneration fg)
        {
            fg.AppendLine($"public {obj.Name}_Observable(IObservable<string> streamSource)");
            using (new DepthWrapper(fg))
            {
                fg.AppendLine(": base(streamSource)");
            }
            using (new BraceWrapper(fg))
            {
            }
            fg.AppendLine();
        }

        private static void AddUsings(FileGeneration fg)
        {
            fg.AppendLine("using Loqui;");
            fg.AppendLine("using Mutagen.Bethesda.Binary;");
            fg.AppendLine("using Mutagen.Bethesda.Oblivion.Internals;");
            fg.AppendLine("using Noggog;");
            fg.AppendLine("using System;");
            fg.AppendLine("using System.Collections.Generic;");
            fg.AppendLine("using System.Linq;");
            fg.AppendLine("using System.Reactive.Linq;");
            fg.AppendLine("using System.Threading.Tasks;");
            fg.AppendLine();
        }

        private async Task GenerateMembers(ObjectGeneration obj, FileGeneration fg)
        {
            foreach (var item in IterateLoqui(obj))
            {
                if (item.IsGroup)
                {
                    fg.AppendLine($"public IObservable<GroupObservable{item.Loqui.GenericTypes}> {item.Loqui.Name} {{ get; private set; }}");
                }
                else
                {
                    fg.AppendLine($"public IObservable<{item.Loqui.TypeName}> {item.Loqui.Name} {{ get; private set; }}");
                }
            }
            fg.AppendLine();
        }

        private async Task GenerateFromPath(ObjectGeneration obj, FileGeneration fg)
        {
            fg.AppendLine($"public static {obj.Name}_Observable FromPath(IObservable<string> streamSource)");
            using (new BraceWrapper(fg))
            {
                fg.AppendLine($"{obj.Name}_Observable ret = new {obj.Name}_Observable(streamSource);");
                foreach (var item in IterateLoqui(obj))
                {
                    if (item.IsGroup)
                    {
                        var grupTargetObj = item.Loqui.GetGroupTarget();
                        fg.AppendLine($"ret.{item.Loqui.Name} = ret.GetGroupObservable<{grupTargetObj.Name}>({grupTargetObj.RegistrationName}.{Mutagen.Bethesda.Constants.TRIGGERING_RECORDTYPE_MEMBER});");
                    }
                }
                fg.AppendLine("return ret;");
            }
            fg.AppendLine();
        }

        private IEnumerable<(LoquiType Loqui, bool IsGroup)> IterateLoqui(ObjectGeneration obj)
        {
            foreach (var item in obj.IterateFields())
            {
                if (!(item is LoquiType loqui))
                {
                    throw new ArgumentException();
                }
                yield return (
                    loqui,
                    loqui.TargetObjectGeneration?.Name.Equals("Group") ?? false);
            }
        }
    }
}