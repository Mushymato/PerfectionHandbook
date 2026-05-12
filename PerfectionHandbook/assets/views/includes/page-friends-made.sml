<lane layout="stretch stretch" orientation="Vertical">
  <include *context={:this} name="mushymato.PerfectionHandbook/views/includes/goal-infobar" />
  <image sprite={@Mods/StardewUI/Sprites/ThinHorizontalDivider} layout="stretch content" margin="0,4,0,0" fit="Stretch"/>
  <panel margin="16" layout="stretch 100%">
    <scrollable peeking="128" scrollbar-margin="-18,0,0,0" progress={<>ScrollProgress}>
      <grid margin="6,0,12,0" item-layout="length: 80+" layout="stretch content">
        <lane *repeat={FilteredDisplayPaginated} orientation="vertical" horizontal-content-alignment="Middle">
          <image layout="80px 80px" fit="Contain" horizontal-alignment="middle" vertical-alignment="end"
            sprite={:MugShotSprite}
            tint={:DisplayTint}/>
          <label layout="128px content" focusable="true" font="small" text={:DisplayName} shadow-alpha="0.8"/>
        </lane>
      </grid>
    </scrollable>
  </panel>
</lane>
