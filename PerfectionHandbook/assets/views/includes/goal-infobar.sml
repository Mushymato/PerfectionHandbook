<lane orientation="Horizontal" *context={:GoalCtx} vertical-content-alignment="Middle">
  <textinput text={<>^SearchText} placeholder={#ui.search} layout="50% 64px" margin="4"/>
  <label text={#ui.showing-completed}
    font="small"
    horizontal-alignment="Middle" 
    margin="2,0"
    shadow-alpha="0.8"
    layout="120px content"
    +state:needed={^ShowNeeded}
    +state:needed:text={#ui.showing-needed}/>
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
