<lane layout="stretch stretch" orientation="Vertical">
  <include *context={:this} name="mushymato.PerfectionHandbook/views/includes/goal-infobar" />
  <image sprite={@Mods/StardewUI/Sprites/ThinHorizontalDivider} layout="stretch content" margin="0,4,0,0" fit="Stretch"/>
  <panel layout="stretch 100%">
    <scrollable peeking="128" scrollbar-margin="-18,0,0,0" progress={<>ScrollProgress}>
      <grid margin="8,0" item-layout="count: 4" layout="stretch content">
        <frame *repeat={FilteredDisplayPaginated}
          layout="content content"
          padding="12"
          focusable="true"
          background={@Mods/StardewUI/Sprites/ShopEntryBorder}>
          <panel>
            <image sprite={@Mods/StardewUI/Sprites/White} tint="#f05461" fit="Stretch" layout={FriendshipFillLayout}/>
            <lane orientation="Horizontal" vertical-content-alignment="Middle">
              <image layout="64px 80px" fit="Contain" horizontal-alignment="middle" vertical-alignment="end"
                sprite={:MugShotSprite}
                tint={:DisplayTint}/>
              <label layout="stretch content" focusable="true" font="dialogue" text={:DisplayName} max-lines="1" shadow-alpha="0.8"/>
            </lane>
          </panel>
        </frame>
      </grid>
    </scrollable>
  </panel>
</lane>
