<panel layout="stretch 100%">
  <scrollable peeking="128" scrollbar-margin="-18,0,0,0" progress={<>ScrollProgress}>
    <grid item-layout="count: 2" item-spacing="-4,-4">
      <frame *repeat={:FilteredDisplayPaginated}
        layout="stretch content"
        padding="12,18"
        focusable="true"
        screen-read={:Description}
        background={@Mods/StardewUI/Sprites/ShopEntryBorder}>
        <lane vertical-content-alignment="Middle">
          <image sprite={:^GoalCtx.DisplayIcon} layout="48px 48px" margin="6" />
          <label text={:Description} shadow-alpha="0.8"/>
        </lane>
      </frame>
    </grid>
  </scrollable>
</panel>
