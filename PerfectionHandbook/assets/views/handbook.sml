<frame layout="1244px 90%[580..]"
  background={@Mods/StardewUI/Sprites/MenuBackground}
  border={@Mods/StardewUI/Sprites/MenuBorder}
  border-thickness="32, 36, 24, 36"
  *switch={PageName}>
  <!-- Main -->
  <scrollable *case="Main">
    <lane layout="stretch content" orientation="Vertical">
      <banner text={#ui.title.perfection}/>
      <goal-grid goals={:PerfectionGoals}/>
      <banner text={#ui.title.achievements}/>
      <goal-grid goals={:AchievementGoals}/>
    </lane>
  </scrollable>
  <!-- Perfection_ItemShipped -->
  <include *case="Perfection_ItemShipped" *context={:SelectedGoalCtx.PageCtx} name="mushymato.PerfectionHandbook/views/includes/page-item-count" />
  <!-- Perfection_RecipesCooked -->
  <include *case="Perfection_RecipesCooked" *context={:SelectedGoalCtx.PageCtx} name="mushymato.PerfectionHandbook/views/includes/page-recipes" />
  <!-- Perfection_RecipesCooked -->
  <include *case="Perfection_RecipesCrafted" *context={:SelectedGoalCtx.PageCtx} name="mushymato.PerfectionHandbook/views/includes/page-recipes" />
  <!-- Perfection_FishCaught -->
  <include *case="Perfection_FishCaught" *context={:SelectedGoalCtx.PageCtx} name="mushymato.PerfectionHandbook/views/includes/page-fish-caught" />
  <!-- Achievement_Museum -->
  <include *case="Achievement_Museum" *context={:SelectedGoalCtx.PageCtx} name="mushymato.PerfectionHandbook/views/includes/page-item-count" />
  <!-- Achievement_Polyculture -->
  <include *case="Achievement_Polyculture" *context={:SelectedGoalCtx.PageCtx} name="mushymato.PerfectionHandbook/views/includes/page-item-count" />
  <!-- Achievement_Monoculture -->
  <include *case="Achievement_Monoculture" *context={:SelectedGoalCtx.PageCtx} name="mushymato.PerfectionHandbook/views/includes/page-item-count" />
</frame>

<template name="goal-grid">
 <grid margin="6,0,16,0" item-layout="count: 4" layout="stretch content">
    <button *repeat={&goals}
      hover-background={@Mods/StardewUI/Sprites/ButtonLight}
      left-click=|^ChangePage(this)|
      horizontal-content-alignment="Start"
      vertical-content-alignment="Start"
      margin="2"
      padding="16,12,0,0"
      layout="stretch 180px">
      <panel layout="100% 100%" >
        <label font="dialogue" text={:Goal.DisplayName} shadow-alpha="0.8" />
        <panel layout="100% 100%" padding="0,0,0,0" horizontal-content-alignment="Start" vertical-content-alignment="End">
          <image sprite={:Goal.DisplayIcon} layout="48px 48px" padding="-4,12"/>
        </panel>
        <panel layout="100% 100%" padding="0,0,16,12" horizontal-content-alignment="End" vertical-content-alignment="End">
          <label font="dialogue" text={:MyFulfillment.DisplayText} />
        </panel>
      </panel>
    </button>
  </grid>
</template>
