<frame layout="80%[1250..] 80%[680..]"
  background={@Mods/StardewUI/Sprites/MenuBackground}
  border={@Mods/StardewUI/Sprites/MenuBorder}
  border-thickness="32, 36, 32, 36"
  *switch={PageName}>
  <!-- Main -->
  <lane *case="Main" margin="6,0,0,0" layout="stretch content" orientation="Vertical">
    <!-- Misc -->
    <lane margin="6,4,16,4" orientation="Horizontal" layout="stretch content">
      <panel layout="stretch content">
        <banner margin="4,12,0,0" text={:PerfectionTitle} />
      </panel>
      <button
        default-background={@Mods/StardewUI/Sprites/MenuSlotOutset}
        hover-background={@Mods/StardewUI/Sprites/MenuSlotInset}
        left-click=|ExportCard()|
        right-click=|OpenCardDir()|
        tooltip={ExportMsg}
        horizontal-content-alignment="Start"
        vertical-content-alignment="Start"
        margin="2"
        padding="12,12,16,0"
        layout="72px 72px">
        <image sprite={@mushymato.PerfectionHandbook/sprites/cursors2:camera} layout="48px 48px" />
      </button>
      <button *repeat={:MiscPages}
        default-background={@Mods/StardewUI/Sprites/MenuSlotOutset}
        hover-background={@Mods/StardewUI/Sprites/MenuSlotInset}
        left-click=|^ChangePage(this)|
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
    <banner margin="10,16" text={#ui.title.achievements}/>
    <goal-grid goals={:AchievementGoals}/>
  </lane>
  <!-- Perfection_ItemShipped -->
  <infobar-page page-name="Perfection_ItemShipped" page-include="mushymato.PerfectionHandbook/views/includes/page-item-count"/>
  <!-- Perfection_RecipesCooked -->
  <infobar-page page-name="Perfection_RecipesCooked" page-include="mushymato.PerfectionHandbook/views/includes/page-item-count"/>
  <!-- Perfection_RecipesCrafted -->
  <infobar-page page-name="Perfection_RecipesCrafted" page-include="mushymato.PerfectionHandbook/views/includes/page-item-count"/>
  <!-- Perfection_FishCaught -->
  <infobar-page page-name="Perfection_FishCaught" page-include="mushymato.PerfectionHandbook/views/includes/page-fish-list"/>
  <!-- Perfection_MonsterSlayered -->
  <infobar-page page-name="Perfection_MonsterSlayered" page-include="mushymato.PerfectionHandbook/views/includes/page-monster-slayer"/>
  <!-- Perfection_BestFriendsMade -->
  <infobar-page page-name="Perfection_BestFriendsMade" page-include="mushymato.PerfectionHandbook/views/includes/page-friends-made"/>
  <!-- Perfection_SkillLeveled -->
  <infobar-page page-name="Perfection_SkillLeveled" page-include="mushymato.PerfectionHandbook/views/includes/page-skills"/>
  <!-- Perfection_BuildingsConstructed -->
  <infobar-page page-name="Perfection_BuildingsConstructed" page-include="mushymato.PerfectionHandbook/views/includes/page-buildings"/>
  <!-- Perfection_StardropsFound -->
  <infobar-page page-name="Perfection_StardropsFound" page-include="mushymato.PerfectionHandbook/views/includes/page-stardrops"/>
  <!-- Perfection_GoldenWalnutsFound -->
  <infobar-page page-name="Perfection_GoldenWalnutsFound" page-include="mushymato.PerfectionHandbook/views/includes/page-golden-walnuts"/>
  <!-- Achievement_CommunityCenter -->
  <infobar-page page-name="Achievement_CommunityCenter" page-include="mushymato.PerfectionHandbook/views/includes/page-community-center"/>
  <!-- Achievement_Museum -->
  <infobar-page page-name="Achievement_Museum" page-include="mushymato.PerfectionHandbook/views/includes/page-item-count"/>
  <!-- Achievement_Polyculture -->
  <infobar-page page-name="Achievement_Polyculture" page-include="mushymato.PerfectionHandbook/views/includes/page-crop-calendar"/>
  <!-- Achievement_Monoculture -->
  <infobar-page page-name="Achievement_Monoculture" page-include="mushymato.PerfectionHandbook/views/includes/page-crop-calendar"/>
  <!-- Misc_Mod_Config -->
  <include *case="Misc_Mod_Config" *context={:SelectedCtx.PageCtx} name="mushymato.PerfectionHandbook/views/includes/mgmt-modconfig" />
  <!-- Misc_Crop_Calendar -->
  <infobar-page page-name="Misc_Crop_Calendar" page-include="mushymato.PerfectionHandbook/views/includes/page-crop-calendar"/>
  <!-- Misc_Required_Ingredients -->
  <infobar-page page-name="Misc_Required_Ingredients" page-include="mushymato.PerfectionHandbook/views/includes/page-ingredient-count"/>
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
          <label font="dialogue" text={:SummaryText} shadow-alpha="0.8" />
        </panel>
      </panel>
    </button>
  </grid>
</template>

<template name="infobar-page">
  <lane *case={&page-name} *context={:SelectedCtx.PageCtx} layout="stretch stretch" orientation="vertical">
    <include name="mushymato.PerfectionHandbook/views/includes/goal-infobar" />
    <image sprite={@Mods/StardewUI/Sprites/ThinHorizontalDivider} layout="stretch content" margin="0,4,0,0" fit="Stretch"/>
    <include name={&page-include} />
  </lane>
</template>
