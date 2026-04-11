<lane orientation="Horizontal" *context={:GoalCtx} vertical-content-alignment="Middle" margin="0,0,16,0">
  <textinput text={<>^SearchText} placeholder={#ui.search} font="dialogue" margin="4,4,0,0" layout="400px stretch" />
  <button hover-background={@Mods/StardewUI/Sprites/ButtonLight}
    *if={:^CanToggleNeeded}
    font="dialogue"
    layout="120px content"
    margin="4,0"
    text={#ui.showing-done}
    left-click=|^ClickShowNeeded()|
    +state:needed={^ShowNeeded}
    +state:needed:text={#ui.showing-need}
  />
  <button hover-background={@Mods/StardewUI/Sprites/ButtonLight}
    *if={:^CanToggleCountMode}
    font="dialogue"
    layout="165px content"
    margin="4,0"
    text={^CountToggleText}
    left-click=|^ClickToggleCount()|
  />
  <frame *repeat={:Fulfillments}
    left-click=|^^ClickFulfilment(this)|
    background={@Mods/StardewUI/Sprites/MenuSlotTransparent}
    background-tint={DisplayTint}
    tooltip={:TooltipText}
    +hover:background-tint="#00000040"
    +transition:background-tint="100ms EaseInSine"
    focusable="true"
    margin="4"
    padding="12">
    <panel>
      <image *if={:HasMiniIcon} padding="0,-5,0,0" sprite={:MiniIcon}/>
      <label margin="48,0,0,0" font="dialogue" text={:DisplayText} />
    </panel>
  </frame>
</lane>
