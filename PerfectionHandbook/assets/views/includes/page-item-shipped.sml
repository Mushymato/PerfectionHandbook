<lane layout="stretch stretch" orientation="Vertical">
  <include *context={:this} name="mushymato.PerfectionHandbook/views/includes/goal-infobar" />
  <image sprite={@Mods/StardewUI/Sprites/ThinHorizontalDivider} layout="stretch content" margin="0,4,8,0" fit="Stretch"/>
  <panel layout="stretch 100%">
    <scrollable peeking="128">
      <grid margin="6,0,0,0" item-layout="length: 72" layout="stretch content">
        <item-icon margin="4" *repeat={FilteredNeedToShip} item={:ReprItem} tooltip={:ReprItem} +hover:scale="1.1"/>
      </grid>
    </scrollable>
  </panel>
</lane>
