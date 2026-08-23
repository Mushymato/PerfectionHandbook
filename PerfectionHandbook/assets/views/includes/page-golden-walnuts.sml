<panel layout="stretch 100%">
  <scrollable peeking="128" scrollbar-margin="-18,0,0,0" progress={<>ScrollProgress}>
    <grid margin="0,0,8,0" item-layout="count: 3" layout="stretch content" item-spacing="-4,-4"
        primary-item-count={>PrimaryItemCount}
        button-press=|HandleShoulderButtons($Button)|>
      <frame *repeat={:FilteredDisplayPaginated}
        padding="12"
        layout="stretch 64px"
        focusable="true"
        tooltip={:Hint}
        background={@Mods/StardewUI/Sprites/ShopEntryBorder}>
        <panel vertical-content-alignment="Middle">
          <image margin="14,-24,0,0" sprite={:^GoalCtx.DisplayIcon} layout="32px 32px"/>
          <label margin="10,36,0,0" font="small" text={:CountText} shadow-alpha="0.8" max-lines="1"/>
          <label margin="64,0,0,0" font="small" text={:Name} shadow-alpha="0.8"/>
        </panel>
      </frame>
    </grid>
  </scrollable>
</panel>
