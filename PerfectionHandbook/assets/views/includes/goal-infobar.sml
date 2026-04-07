<lane orientation="Horizontal" *context={:GoalCtx} vertical-content-alignment="Middle">
  <textinput text={<>^SearchText} placeholder={#ui.search} font="dialogue" layout="40% 68px" margin="4,4,4,0"/>
  <button hover-background={@Mods/StardewUI/Sprites/ButtonLight}
    font="dialogue"
    layout="120px content"
    margin="4,0"
    text={#ui.showing-done}
    left-click=|^ClickShowNeeded()|
    +state:needed={^ShowNeeded}
    +state:needed:text={#ui.showing-need}
  />
  <button hover-background={@Mods/StardewUI/Sprites/ButtonLight}
    left-click=|^ClickMyFulfilment()|
    *context={:MyFulfillment}>
    <lane tooltip={:TooltipText}>
      <image *if={:HasMiniIcon} padding="0,-5,0,0" sprite={:MiniIcon}/>
      <label font="dialogue" text={:DisplayText} shadow-alpha="0.8" />
    </lane>
  </button>
  <button *if={:ShowBestFulfillment}
    hover-background={@Mods/StardewUI/Sprites/ButtonLight}
    left-click=|^ClickBestFulfilment()|
    *context={:BestFulfillment}>
    <lane tooltip={:TooltipText}>
      <image *if={:HasMiniIcon} padding="0,-5,0,0" sprite={:MiniIcon}/>
      <label font="dialogue" text={:DisplayText} shadow-alpha="0.8" />
    </lane>
  </button>
</lane>
