using System;
using System.Collections.Generic;
using Skclusive.Mobx.StateTree;
using Xunit;
using static Skclusive.Mobx.StateTree.Tests.TestTypes;

namespace Skclusive.Mobx.StateTree.Tests
{
    public class TestNested
    {
        [Fact]
        public void TestRoot()
        {
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
        }
    }
}
