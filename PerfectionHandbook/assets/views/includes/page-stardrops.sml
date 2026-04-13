<lane layout="stretch stretch" orientation="Vertical">
  <include *context={:this} name="mushymato.PerfectionHandbook/views/includes/goal-infobar" />
  <image sprite={@Mods/StardewUI/Sprites/ThinHorizontalDivider} layout="stretch content" margin="0,4,0,0" fit="Stretch"/>
  <panel layout="stretch 100%">
    <scrollable peeking="128" scrollbar-margin="-18,0,0,0" progress={<>ScrollProgress}>
      <lane orientation="Vertical">
        <frame *repeat={FilteredDisplayPaginated}
          layout="stretch content"
          padding="12"
          focusable="true"
          background={@Mods/StardewUI/Sprites/ShopEntryBorder}>
          <lane vertical-content-alignment="Middle">
            <image sprite={:^GoalCtx.DisplayIcon} layout="48px 48px" margin="6" />
            <label font="dialogue" text={:Description} shadow-alpha="1"/>
          </lane>
        </frame>
      </lane>
    </scrollable>
  </panel>
</lane>
