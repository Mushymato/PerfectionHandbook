<lane layout="stretch stretch" orientation="Vertical">
  <include *context={:this} name="mushymato.PerfectionHandbook/views/includes/goal-infobar" />
  <image sprite={@Mods/StardewUI/Sprites/ThinHorizontalDivider} layout="stretch content" margin="0,4,8,0" fit="Stretch"/>
  <panel layout="stretch 100%">
    <scrollable peeking="128" scroll-step="108" scrollbar-margin="-26,0,0,0">
      <grid margin="6,0,0,0" item-layout="length: 80" layout="stretch content">
        <item-icon margin="4"
          *repeat={FilteredRecipes}
          item={:Info.ReprItem}
          tooltip={:Tooltip}
          tint={:DisplayTint}
          +hover:scale="1.1"
        />
      </grid>
    </scrollable>
  </panel>
</lane>
