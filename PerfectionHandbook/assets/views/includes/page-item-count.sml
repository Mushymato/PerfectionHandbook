<lane layout="stretch stretch" orientation="Vertical">
  <include *context={:this} name="mushymato.PerfectionHandbook/views/includes/goal-infobar" />
  <image sprite={@Mods/StardewUI/Sprites/ThinHorizontalDivider} layout="stretch content" margin="0,4,8,0" fit="Stretch"/>
  <panel layout="stretch 100%">
    <scrollable peeking="128" scrollbar-margin="-26,0,0,0"
      progress={<>ScrollProgress}>
      <grid margin="6,0,0,0" item-layout="length: 80" layout="stretch content">
        <item-icon *repeat={FilteredDisplayPaginated}
          item={:ReprItem}
          tooltip={:Tooltip}
          tint={DisplayTint}
          focusable="true"
          padding="4"
          +hover:scale="1.1"
          +transition:scale="100ms EaseInSine"
        />
      </grid>
    </scrollable>
  </panel>
</lane>
