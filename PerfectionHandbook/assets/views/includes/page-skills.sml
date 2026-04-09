<lane layout="stretch stretch" orientation="Vertical">
  <include *context={:this} name="mushymato.PerfectionHandbook/views/includes/goal-infobar" />
  <image sprite={@Mods/StardewUI/Sprites/ThinHorizontalDivider} layout="stretch content" margin="0,4,8,0" fit="Stretch"/>
  <panel layout="stretch 100%">
    <scrollable peeking="128" progress={<>ScrollProgress}>
      <grid margin="6,0,12,0" item-layout="count: 2" layout="stretch content">
        <frame *repeat={FilteredDisplayPaginated}
          layout="100% content"
          border-thickness="12"
          focusable="true"
          border={@mushymato.PerfectionHandbook/sprites/cursors:shopBg}>
          <lane orientation="Vertical">
            <lane padding="4,4,4,0" orientation="Horizontal" layout="400px content" vertical-content-alignment="Middle">
              <image sprite={:SkillIcon} margin="8" layout="64px 64px"/>
              <label font="dialogue" text={:DisplayCounts} max-lines="1" shadow-alpha="0.5" margin="4,0" />
              <label font="dialogue" text={:SkillName} max-lines="1" shadow-alpha="0.5" margin="4,0" />
            </lane>
            <panel>
              <lane *repeat={ExpToNextFillLayouts} padding="4,0,4,4" orientation="Horizontal" layout="100% content" horizontal-content-alignment="Start" vertical-content-alignment="Middle">
                <frame *repeat={:this} layout="40px 24px" margin="4" border-thickness="4" border={@Mods/StardewUI/Sprites/MenuSlotTransparent}>
                  <image *if={:Show}
                    sprite={@Mods/StardewUI/Sprites/White}
                    horizontal-alignment="Start"
                    tint={:Tint}
                    fit="Stretch"
                    layout={:Layout}
                  />
                </frame>
              </lane>
            </panel>
          </lane>
        </frame>
      </grid>
    </scrollable>
  </panel>
</lane>
