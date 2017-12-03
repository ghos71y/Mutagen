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
    class Program
    {
        static void Main(string[] args)
        {
            LoquiGenerator gen = new LoquiGenerator(
                new DirectoryInfo("../../../Mutagen.Oblivion"))
            {
                DefaultNamespace = "Mutagen.Oblivion",
                RaisePropertyChangedDefault = false,
                ProtocolDefault = new ProtocolKey("Oblivion"),
                NotifyingDefault = NotifyingOption.Notifying
            };
            gen.Namespaces.Add("Mutagen.Bethesda");
            gen.XmlTranslation.ShouldGenerateXSD = false;
            gen.XmlTranslation.AddTypeAssociation<FormIDType>(new PrimitiveXmlTranslationGeneration<FormID>());
            gen.GenerationModules.Add(new MutagenModule());
            gen.Add(new BinaryTranslationModule(gen));
            gen.AddTypeAssociation<FormIDType>("FormID");
            gen.AddTypeAssociation<BufferType>("Buffer");
            gen.AddTypeAssociation<DataType>("Data");
            gen.ReplaceTypeAssociation<Loqui.Generation.EnumType, Mutagen.Bethesda.Generation.EnumType>();
            gen.ReplaceTypeAssociation<Loqui.Generation.StringType, Mutagen.Bethesda.Generation.StringType>();

            // Add Projects
            gen.AddProjectToModify(
                new FileInfo(Path.Combine(gen.CommonGenerationFolder.FullName, "Mutagen.Oblivion.csproj")));

            gen.AddProtocol(
                new ProtocolGeneration(
                    gen,
                    new ProtocolKey("Oblivion")));

            gen.Generate().Wait();
        }
    }
}