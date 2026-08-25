<lane orientation="Horizontal" *context={:GoalCtx} vertical-content-alignment="Middle" layout="stretch content">
  <!-- Sort -->
  <panel margin="8,0,0,0" layout="content stretch" vertical-content-alignment="middle" *if={:^HasSortModes} *context={:^SortModeCtx} >
    <panel focusable="true" tooltip={ValueLabel} left-click=|Increase()| right-click=|Decrease()|>
      <image sprite={@mushymato.PerfectionHandbook/sprites/cursors2:dotdotdot} layout="64px 64px"/>
      <image sprite={@mushymato.PerfectionHandbook/sprites/cursors:organize} layout="40px 48px" margin="12,10,0,0" +hover:scale="1.2" +transition:scale="100ms EaseInSine"/>
    </panel>
  </panel>
  <!-- Search Bar -->
  <textinput text={<>^SearchText} placeholder={#ui.search} margin="0,4,0,0" layout="260px content" />
  <!-- Swtich Modes -->
  <lane orientation="vertical" margin="4,0" layout="content content">
    <two-segment *if={:^CanToggleNeeded}
      binding={<>^NeededIndex}
      option1={#ui.showing-need}
      option2={#ui.showing-done} />
    <two-segment *if={:^CanToggleCountMode}
      binding={<>^CountModeIndex}
      option1={#ui.counting-owned}
      option2={:^CompleteCountToggleText} />
  </lane>
  <!-- Farmer Pick -->
  <frame *repeat={:Fulfillments}
    left-click=|^^ClickFulfilment(this)|
    border={@Mods/StardewUI/Sprites/MenuSlotTransparent}
    border-tint={DisplayTint}
    tooltip={:TooltipText}
    +hover:border-tint="#00000040"
    +transition:border-tint="100ms EaseInSine"
    focusable="true"
    layout="content content"
    padding="4,0,4,4"
    vertical-content-alignment="middle">
    <lane *switch={:HasMiniIcon} vertical-content-alignment="middle">
      <image *case="true" layout="48px 48px" fit="Contain" vertical-alignment="end" sprite={:MiniIcon}/>
      <image *case="false" layout="48px 48px" fit="Contain" vertical-alignment="end" sprite={:~HandbookContext.FarmIcon}/>
      <label *if={Selected} margin="0,0,8,0" text={:DisplayText} shadow-alpha="0.4" max-lines="-1"/>
    </lane>
  </frame>
  <!-- paginator -->
  <panel *if={^HasPagination} layout="stretch content" horizontal-content-alignment="End">
    <lane orientation="horizontal" margin="0,0"
      vertical-content-alignment="Middle"
      button-press=|^HandleShoulderButtons($Button)|>
      <image sprite={@Mods/StardewUI/Sprites/LargeLeftArrow}
        focusable="true"
        left-click=|^PaginatePrev()|
        opacity={^PrevPaginateButtonOpacity}
        screen-read={#ui.prev-page}
        +hover:scale="1.2"/>
      <banner text={^ScrollPage} layout="content content"/>
      <image sprite={@Mods/StardewUI/Sprites/LargeRightArrow}
        focusable="true"
        left-click=|^PaginateNext()|
        opacity={^NextPaginateButtonOpacity}
        screen-read={#ui.next-page}
        +hover:scale="1.2"/>
    </lane>
  </panel>
</lane>

<template name="two-segment">
  <frame margin="0,-2" border={@Mods/StardewUI/Sprites/ScrollBarTrack}>
    <segments balanced="true"
        highlight={@Mods/StardewUI/Sprites/ButtonDark}
        highlight-transition="150ms EaseOutQuart"
        selected-index={&binding}>
      <panel layout="112px content" horizontal-content-alignment="middle">
        <label margin="4,8" text={&option1} />
      </panel>
      <panel layout="112px content" horizontal-content-alignment="middle">
        <label margin="4,8" text={&option2}/>
      </panel>
    </segments>
  </frame>
</template>
