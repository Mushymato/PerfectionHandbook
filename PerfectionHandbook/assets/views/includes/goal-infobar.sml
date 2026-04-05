<lane orientation="Horizontal" *context={:GoalContext} vertical-content-alignment="Middle">
  <textinput text={<>^SearchText} placeholder={#ui.search} layout="50% 56px" margin="4"/>
  <button margin="4,0" hover-background={@Mods/StardewUI/Sprites/ButtonLight}
    left-click=|^ClickMyFulfilment()|
    *context={:MyFulfillment}>
    <lane tooltip={:TooltipText}>
      <image *if={:HasMiniIcon} sprite={:MiniIcon}/>
      <label font="dialogue" text={:DisplayText} shadow-alpha="0.8" />
    </lane>
  </button>
  <button *if={:ShowBestFulfillment}
    hover-background={@Mods/StardewUI/Sprites/ButtonLight}
    left-click=|^ClickBestFulfilment()|
    *context={:BestFulfillment}>
    <lane tooltip={:TooltipText}>
      <image *if={:HasMiniIcon} sprite={:MiniIcon}/>
      <label font="dialogue" text={:DisplayText} shadow-alpha="0.8" />
    </lane>
  </button>
</lane>
