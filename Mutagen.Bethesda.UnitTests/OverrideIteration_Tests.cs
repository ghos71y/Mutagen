using Mutagen.Bethesda.Skyrim;
using Noggog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;
using FluentAssertions;
using FluentAssertions.Common;

namespace Mutagen.Bethesda.UnitTests
{
    public class Override_Tests
    {
        [Fact]
        public void WinningOverrides_Typical()
        {
            var master = new SkyrimMod(new ModKey("Base", ModType.Master), SkyrimRelease.SkyrimSE);
            var baseNpc = master.Npcs.AddNew();
            var otherMasterNpc = master.Npcs.AddNew();
            var plugin = new SkyrimMod(new ModKey("Plugin", ModType.Plugin), SkyrimRelease.SkyrimSE);
            var pluginNpc = plugin.Npcs.AddNew();
            var overrideNpc = (Npc)baseNpc.DeepCopy();
            plugin.Npcs.RecordCache.Set(overrideNpc);
            overrideNpc.Name = "Override";

            var winningOverrides = plugin.AsEnumerable().And(master)
                .WinningOverrides<Npc>()
                .ToDictionary(n => n.FormKey);

            Assert.Equal(3, winningOverrides.Count);
            Assert.Same(otherMasterNpc, winningOverrides[otherMasterNpc.FormKey]);
            Assert.Same(pluginNpc, winningOverrides[pluginNpc.FormKey]);
            Assert.Equal(overrideNpc.FormKey, baseNpc.FormKey);
            Assert.Same(overrideNpc, winningOverrides[baseNpc.FormKey]);
        }

        [Fact]
        public void Worldspace_GetOrAddAsOverride_Clean()
        {
            WarmupSkyrim.Init();
            var mod = new SkyrimMod(Utility.ModKey, SkyrimRelease.SkyrimSE);
            var worldspace = mod.Worldspaces.AddNew();
            var block = worldspace.SubCells.AddReturn(new WorldspaceBlock());
            var subBlock = block.Items.AddReturn(new WorldspaceSubBlock());
            var cell = subBlock.Items.AddReturn(new Cell(mod.GetNextFormKey()));

            var mod2 = new SkyrimMod(Utility.ModKey2, SkyrimRelease.SkyrimSE);
            var worldspaceOverride = mod2.Worldspaces.GetOrAddAsOverride(worldspace);
            Assert.Empty(worldspaceOverride.SubCells);
        }

        [Fact]
        public void Worldspace_WinningContextOverride_IPlaced()
        {
            WarmupSkyrim.Init();
            var mod = new SkyrimMod(Utility.ModKey, SkyrimRelease.SkyrimSE);
            var worldspace = mod.Worldspaces.AddNew();
            var block = worldspace.SubCells.AddReturn(new WorldspaceBlock());
            var subBlock = block.Items.AddReturn(new WorldspaceSubBlock());
            var cell = subBlock.Items.AddReturn(new Cell(mod.GetNextFormKey()));
            var placedObj = cell.Persistent.AddReturn(new PlacedObject(mod.GetNextFormKey()));
            worldspace.TopCell = new Cell(mod.GetNextFormKey());
            var placedObj2 = worldspace.TopCell.Persistent.AddReturn(new PlacedObject(mod.GetNextFormKey()));
            var placedObjs = mod.EnumerateMajorRecordContexts<IPlacedObject, IPlacedObjectGetter>().ToList();
            placedObjs.Should().HaveCount(2);
            var placed = mod.EnumerateMajorRecordContexts<IPlaced, IPlacedGetter>().ToList();
            placed.Should().HaveCount(2);
        }
    }
}
