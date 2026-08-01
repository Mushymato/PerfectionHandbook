<frame layout="1440px 620px" background={@Mods/StardewUI/Sprites/MenuBackground}>
  <!-- Main -->
  <lane layout="content content">
    <lane margin="16" orientation="Vertical" layout="content stretch" horizontal-content-alignment="Middle" >
      <image layout="128px 192px" sprite={:FarmerPanel} />
      <banner text={:PlayerName} />
      <panel layout="content stretch" vertical-content-alignment="End">
        <image layout="88px 80px" sprite={:FarmIcon} shadow-alpha="0.4" shadow-offset="-4,4"/>
      </panel>
      <label margin="0,8" text={:FarmName} shadow-alpha="0.8" />
    </lane>
    <image sprite={@Mods/StardewUI/Sprites/ThinVerticalDivider} margin="8,0,16,0" layout="content stretch" fit="Stretch"/>
    <lane layout="stretch content" orientation="Vertical">
      <banner margin="10,16" text={:PerfectionTitle} />
      <goal-grid goals={:PerfectionGoals}/>
      <banner margin="10,16" text={#ui.title.achievements}/>
      <goal-grid goals={:AchievementGoals}/>
    </lane>
  </lane>
</frame>

<template name="goal-grid">
 <grid margin="6,0,16,0" item-layout="count: 5" layout="stretch content">
    <button *repeat={&goals}
      default-background={@Mods/StardewUI/Sprites/MenuSlotOutset}
      hover-background={@Mods/StardewUI/Sprites/MenuSlotInset}
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
