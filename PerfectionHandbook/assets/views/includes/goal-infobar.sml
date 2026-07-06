<lane orientation="Horizontal" *context={:GoalCtx} vertical-content-alignment="Middle" margin="0,0,16,0">
  <textinput text={<>^SearchText} placeholder={#ui.search} font="dialogue" margin="4,4,0,0" layout="400px content" />
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
  <panel *if={:^HasSortModes} *context={:^SortModeCtx} >
    <panel focusable="true" margin="12,0,0,0" tooltip={ValueLabel} left-click=|Increase()| right-click=|Decrease()|>
      <image sprite={@mushymato.PerfectionHandbook/sprites/cursors2:dotdotdot} layout="64px 64px"/>
      <image sprite={@mushymato.PerfectionHandbook/sprites/cursors:organize} layout="40px 48px" margin="12,10,0,0" +hover:scale="1.2" +transition:scale="100ms EaseInSine"/>
    </panel>
  </panel>
  <button hover-background={@Mods/StardewUI/Sprites/ButtonLight}
    *if={:^CanToggleCountMode}
    font="dialogue"
    layout="180px content"
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
    <lane>
      <image *if={:HasMiniIcon} padding="0,-5,0,0" sprite={:MiniIcon}/>
      <label font="dialogue" text={:DisplayText} />
    </lane>
  </frame>
</lane>
