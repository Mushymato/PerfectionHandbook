<frame layout="80%[1280..] 80%[700..]"
  background={@Mods/StardewUI/Sprites/MenuBackground}
  border={@Mods/StardewUI/Sprites/MenuBorder}
  border-thickness="32, 36, 32, 36"
  *switch={PageName}>
  <!-- Main -->
  <scrollable scrollbar-margin="-18,0,0,0" *case="Main">
    <lane layout="stretch content" orientation="Vertical">
      <!-- Misc -->
      <lane margin="6,4,16,4" orientation="Horizontal" layout="stretch content">
        <panel layout="stretch content">
          <banner margin="16,8" text={#ui.title.perfection} />
        </panel>
        <button *repeat={:MiscPages}
          default-background={@Mods/StardewUI/Sprites/MenuSlotOutset}
          hover-background={@Mods/StardewUI/Sprites/MenuSlotInset}
          left-click=|^ChangePage(this)|
          screen-read={:DisplayName}
          tooltip={:DisplayName}
          horizontal-content-alignment="Start"
          vertical-content-alignment="Start"
          margin="2"
          padding="12,12,16,0"
          layout="72px 72px">
          <image sprite={:DisplayIcon} layout="48px 48px" />
        </button>
      </lane>
      <!-- Goals -->
      <goal-grid goals={:PerfectionGoals}/>
      <banner margin="16,8" text={#ui.title.achievements}/>
      <goal-grid goals={:AchievementGoals}/>
    </lane>
  </scrollable>
  <!-- Perfection_ItemShipped -->
  <include *case="Perfection_ItemShipped" *context={:SelectedCtx.PageCtx} name="mushymato.PerfectionHandbook/views/includes/page-item-count" />
  <!-- Perfection_RecipesCooked -->
  <include *case="Perfection_RecipesCooked" *context={:SelectedCtx.PageCtx} name="mushymato.PerfectionHandbook/views/includes/page-item-count" />
  <!-- Perfection_RecipesCrafted -->
  <include *case="Perfection_RecipesCrafted" *context={:SelectedCtx.PageCtx} name="mushymato.PerfectionHandbook/views/includes/page-item-count" />
  <!-- Perfection_FishCaught -->
  <include *case="Perfection_FishCaught" *context={:SelectedCtx.PageCtx} name="mushymato.PerfectionHandbook/views/includes/page-item-count" />
  <!-- Perfection_MonsterSlayered -->
  <include *case="Perfection_MonsterSlayered" *context={:SelectedCtx.PageCtx} name="mushymato.PerfectionHandbook/views/includes/page-monster-slayer" />
  <!-- Perfection_BestFriendsMade -->
  <include *case="Perfection_BestFriendsMade" *context={:SelectedCtx.PageCtx} name="mushymato.PerfectionHandbook/views/includes/page-friends-made" />
  <!-- Perfection_SkillLeveled -->
  <include *case="Perfection_SkillLeveled" *context={:SelectedCtx.PageCtx} name="mushymato.PerfectionHandbook/views/includes/page-skills" />
  <!-- Perfection_BuildingsConstructed -->
  <include *case="Perfection_BuildingsConstructed" *context={:SelectedCtx.PageCtx} name="mushymato.PerfectionHandbook/views/includes/page-buildings" />
  <!-- Perfection_StardropsFound -->
  <include *case="Perfection_StardropsFound" *context={:SelectedCtx.PageCtx} name="mushymato.PerfectionHandbook/views/includes/page-stardrops" />
  <!-- Achievement_Museum -->
  <include *case="Achievement_Museum" *context={:SelectedCtx.PageCtx} name="mushymato.PerfectionHandbook/views/includes/page-item-count" />
  <!-- Achievement_Polyculture -->
  <include *case="Achievement_Polyculture" *context={:SelectedCtx.PageCtx} name="mushymato.PerfectionHandbook/views/includes/page-crop-calendar" />
  <!-- Achievement_Monoculture -->
  <include *case="Achievement_Monoculture" *context={:SelectedCtx.PageCtx} name="mushymato.PerfectionHandbook/views/includes/page-crop-calendar" />
  <!-- Misc_Mod_Config -->
  <include *case="Misc_Mod_Config" *context={:SelectedCtx.PageCtx} name="mushymato.PerfectionHandbook/views/includes/mgmt-modconfig" />
  <!-- Misc_Crop_Calendar -->
  <include *case="Misc_Crop_Calendar" *context={:SelectedCtx.PageCtx} name="mushymato.PerfectionHandbook/views/includes/page-crop-calendar" />
  <!-- Misc_Required_Ingredients -->
  <include *case="Misc_Required_Ingredients" *context={:SelectedCtx.PageCtx} name="mushymato.PerfectionHandbook/views/includes/page-ingredient-count" />
</frame>

<template name="goal-grid">
 <grid margin="6,0,16,0" item-layout="count: 5" layout="stretch content">
    <button *repeat={&goals}
      default-background={@Mods/StardewUI/Sprites/MenuSlotOutset}
      hover-background={@Mods/StardewUI/Sprites/MenuSlotInset}
      left-click=|^ChangePage(this)|
      screen-read={:DisplayName}
      horizontal-content-alignment="Start"
      vertical-content-alignment="Start"
      margin="2"
      padding="12,12,16,0"
      layout="stretch 144px">
      <panel layout="100% 100%" >
        <image sprite={:DisplayIcon} layout="48px 48px" />
        <label margin="56,0,0,0" font="small" text={:DisplayName} max-lines="3" shadow-alpha="0.8" />
        <panel layout="100% 100%" padding="0,0,0,12" horizontal-content-alignment="End" vertical-content-alignment="End">
          <label font="dialogue" text={:SummaryText} shadow-alpha="0.8"  />
        </panel>
      </panel>
    </button>
  </grid>
</template>
