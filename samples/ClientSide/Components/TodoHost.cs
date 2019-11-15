using ClientSide.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Skclusive.Core.Component;

namespace ClientSide.Components
{
    public class TodoHostComponent : PureComponentBase
    {
        [Parameter]
        public RenderFragment ChildContent { get; set; }

        protected ITodoStore TodoStore { get; set; }

        public TodoHostComponent()
        {
            TodoStore = ModelTypes.StoreType.Create(new TodoStoreSnapshot
            {
                Filter = "ShowAll",

                Todos = new ITodoSnapshot[]
                {
                    new TodoSnapshot { Title = "Get coffee" },

                    new TodoSnapshot { Title = "Learn Blazor" }
                }
            });
        }
    }
}
