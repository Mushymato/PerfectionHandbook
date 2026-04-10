<frame layout="80%[1280..] 85%[700..]"
  background={@Mods/StardewUI/Sprites/MenuBackground}
  border={@Mods/StardewUI/Sprites/MenuBorder}
  border-thickness="32, 36, 24, 36"
  *switch={PageName}>
  <!-- Main -->
  <scrollable *case="Main">
    <lane layout="stretch content" orientation="Vertical">
      <banner margin="16,8" text={#ui.title.perfection}/>
      <goal-grid goals={:PerfectionGoals}/>
      <banner margin="16,8" text={#ui.title.achievements}/>
      <goal-grid goals={:AchievementGoals}/>
    </lane>
  </scrollable>
  <!-- Perfection_ItemShipped -->
  <include *case="Perfection_ItemShipped" *context={:SelectedGoalCtx.PageCtx} name="mushymato.PerfectionHandbook/views/includes/page-item-count" />
  <!-- Perfection_RecipesCooked -->
  <include *case="Perfection_RecipesCooked" *context={:SelectedGoalCtx.PageCtx} name="mushymato.PerfectionHandbook/views/includes/page-item-count" />
  <!-- Perfection_RecipesCooked -->
  <include *case="Perfection_RecipesCrafted" *context={:SelectedGoalCtx.PageCtx} name="mushymato.PerfectionHandbook/views/includes/page-item-count" />
  <!-- Perfection_FishCaught -->
  <include *case="Perfection_FishCaught" *context={:SelectedGoalCtx.PageCtx} name="mushymato.PerfectionHandbook/views/includes/page-item-count" />
  <!-- Perfection_MonsterSlayered -->
  <include *case="Perfection_MonsterSlayered" *context={:SelectedGoalCtx.PageCtx} name="mushymato.PerfectionHandbook/views/includes/page-monster-slayer" />
  <!-- Perfection_SkillLeveled -->
  <include *case="Perfection_SkillLeveled" *context={:SelectedGoalCtx.PageCtx} name="mushymato.PerfectionHandbook/views/includes/page-skills" />
  <!-- Perfection_BuildingsConstructed -->
  <include *case="Perfection_BuildingsConstructed" *context={:SelectedGoalCtx.PageCtx} name="mushymato.PerfectionHandbook/views/includes/page-buildings-constructed" />
  <!-- Achievement_Museum -->
  <include *case="Achievement_Museum" *context={:SelectedGoalCtx.PageCtx} name="mushymato.PerfectionHandbook/views/includes/page-item-count" />
  <!-- Achievement_Polyculture -->
  <include *case="Achievement_Polyculture" *context={:SelectedGoalCtx.PageCtx} name="mushymato.PerfectionHandbook/views/includes/page-crop-calendar" />
  <!-- Achievement_Monoculture -->
  <include *case="Achievement_Monoculture" *context={:SelectedGoalCtx.PageCtx} name="mushymato.PerfectionHandbook/views/includes/page-crop-calendar" />
</frame>

<template name="goal-grid">
 <grid margin="6,0,16,0" item-layout="count: 5" layout="stretch content">
    <button *repeat={&goals}
      default-background={@Mods/StardewUI/Sprites/MenuSlotOutset}
      hover-background={@Mods/StardewUI/Sprites/MenuSlotInset}
      left-click=|^ChangePage(this)|
      screen-read={:Goal.DisplayName}
      horizontal-content-alignment="Start"
      vertical-content-alignment="Start"
      margin="2"
      padding="12,12,16,0"
      layout="stretch 140px">
      <panel layout="100% 100%" >
        <image sprite={:Goal.DisplayIcon} layout="48px 48px" />
        <label margin="56,0,0,0" font="small" text={:Goal.DisplayName} max-lines="2" shadow-alpha="0.8" />
        <panel layout="100% 100%" padding="0,0,0,12" horizontal-content-alignment="End" vertical-content-alignment="End">
          <label font="dialogue" text={:BestFulfillment.DisplayText} shadow-alpha="0.8"  />
        </panel>
      </panel>
    </button>
  </grid>
</template>
