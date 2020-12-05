using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Skclusive.Mobx.StateTree;
using Skclusive.Text.Json;
using Xunit;
using static Skclusive.Mobx.StateTree.Tests.TestTypes;

namespace Skclusive.Mobx.StateTree.Tests
{
    public class TestNested
    {
        [Fact]
        public void TestRoot()
        {
            var services = new ServiceCollection();
            services.TryAddJsonServices();
            services.TryAddJsonTypeConverter<IRootSnapshot, RootSnapshot>();
            services.TryAddJsonTypeConverter<IBranchSnapshot, BranchSnapshot>();
            services.TryAddJsonTypeConverter<ITreeSnapshot, TreeSnapshot>();

            var jsonService = services.BuildServiceProvider().GetService<IJsonService>();

            var root = RootType.Create(new RootSnapshot
            {
                Tree = new TreeSnapshot
                {
                    Branches = new IBranchSnapshot []
                    {
                        new BranchSnapshot { Name = "branch 1" },

                        new BranchSnapshot { Name = "branch 2" }
                    }
                }
            });

            Assert.NotNull(root);

            Assert.NotNull(root.Tree);

            Assert.Equal(2, root.Tree.Branches.Count);

            Assert.Equal("branch 1", root.Tree.Branches[0].Name);

            Assert.Equal("branch 2", root.Tree.Branches[1].Name);

            root.Tree.AddBranch(new BranchSnapshot { Name = "branch 4 (typo)" });

            Assert.Equal(3, root.Tree.Branches.Count);

            Assert.Equal("branch 4 (typo)", root.Tree.Branches[2].Name);

            root.Tree.Branches[2].EditName("branch 3");

            Assert.Equal("branch 3", root.Tree.Branches[2].Name);

            var rootSnapshot = new RootSnapshot
            {
                Tree = new TreeSnapshot
                {
                    Branches = new IBranchSnapshot[]
                    {
                        new BranchSnapshot { Name = "snap branch 1" },

                        new BranchSnapshot { Name = "snap branch 2" }
                    }
                }
            };

            root.ApplySnapshot(rootSnapshot);

            Assert.Equal(2, root.Tree.Branches.Count);

            Assert.Equal("snap branch 1", root.Tree.Branches[0].Name);

            Assert.Equal("snap branch 2", root.Tree.Branches[1].Name);

            var jsonSnapshot = jsonService.Serialize(rootSnapshot);

            var jsonSerialized = jsonService.Deserialize<RootSnapshot>(jsonSnapshot);

            root.ApplySnapshot(jsonSerialized);

            Assert.Equal(2, root.Tree.Branches.Count);

            Assert.Equal("snap branch 1", root.Tree.Branches[0].Name);

            Assert.Equal("snap branch 2", root.Tree.Branches[1].Name);
        }
    }
}
