<lane layout="stretch stretch" orientation="Vertical">
  <include *context={:this} name="mushymato.PerfectionHandbook/views/includes/goal-infobar" />
  <image sprite={@Mods/StardewUI/Sprites/ThinHorizontalDivider} layout="stretch content" margin="-6,4,0,0" fit="Stretch"/>
  <panel layout="stretch 100%">
    <scrollable peeking="128" scrollbar-margin="-18,0,0,0" progress={<>ScrollProgress}>
      <grid margin="8,0" item-layout="length: 400+" layout="stretch content">
        <frame *repeat={FilteredDisplayPaginated}
          layout="content content"
          padding="12"
          focusable="true"
          background={@Mods/StardewUI/Sprites/ShopEntryBorder}>
          <panel vertical-content-alignment="End">
            <lane orientation="Horizontal" vertical-content-alignment="Middle">
              <image layout="64px 80px"
                fit="Contain"
                horizontal-alignment="middle"
                vertical-alignment="end"
                sprite={:MugShotSprite}
                tint={:DisplayTint}/>
              <lane orientation="Vertical" margin="0,0,12,0" >
                <label layout="stretch content" margin="4,0,0,8" focusable="true" font="small" text={:DisplayName} max-lines="1" shadow-alpha="0.8"/>
                <frame layout="100% 24px" margin="8,0,0,0" border-thickness="4" border={@Mods/StardewUI/Sprites/MenuSlotTransparent} focusable="true">
                  <panel layout="100% stretch" vertical-content-alignment="End">
                    <image sprite={@mushymato.PerfectionHandbook/sprites/cursors:heartFillPx} fit="Stretch" layout={FriendshipFillLayout}/>
                    <image sprite={@mushymato.PerfectionHandbook/sprites/cursors:heartFill} margin="-14,0,0,0" layout="28px 24px"/>
                    <label text={HeartLevel} margin="16,0,0,0" horizontal-alignment="Start" shadow-alpha="1" />
                    <label text={FriendshipPointDisplay} horizontal-alignment="End" layout="stretch content" shadow-alpha="1" />
                  </panel>
                </frame>
              </lane>
            </lane>
          </panel>
        </frame>
      </grid>
    </scrollable>
  </panel>
</lane>
