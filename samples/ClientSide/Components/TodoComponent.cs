using ClientSide.Models;
using Microsoft.AspNetCore.Components;
using Skclusive.Mobx.Component;

namespace ClientSide.Components
{
    public class TodoComponent : ObservableComponentBase
    {
        [Parameter]
        public RenderFragment ChildContent { get; set; }

        [CascadingParameter]
        public ITodoStore TodoStore { get; set; }
    }
}
