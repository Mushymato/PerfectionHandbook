<lane orientation="Horizontal" *context={:GoalContext} vertical-content-alignment="Middle">
  <textinput text={<>^SearchText} placeholder={#ui.search} layout="50% 56px" margin="4"/>
  <button margin="4,0" hover-background={@Mods/StardewUI/Sprites/ButtonLight}
    left-click=|^ClickMyFulfilment()|>
    <lane>
      <image *if={:MyFulfillment.HasMiniIcon} sprite={:MyFulfillment.MiniIcon}/>
      <label font="dialogue" text={:MyFulfillment.DisplayText} shadow-alpha="0.8" />
    </lane>
  </button>
  <button *if={:ShowBestFulfillment}
    hover-background={@Mods/StardewUI/Sprites/ButtonLight}
    left-click=|^ClickBestFulfilment()|>
    <lane>
      <image sprite={:BestFulfillment.MiniIcon}/>
      <label font="dialogue" text={:BestFulfillment.DisplayText} shadow-alpha="0.8" />
    </lane>
  </button>
</lane>
