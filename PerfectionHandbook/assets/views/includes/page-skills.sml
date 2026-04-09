<lane layout="stretch stretch" orientation="Vertical">
  <include *context={:this} name="mushymato.PerfectionHandbook/views/includes/goal-infobar" />
  <image sprite={@Mods/StardewUI/Sprites/ThinHorizontalDivider} layout="stretch content" margin="0,4,8,0" fit="Stretch"/>
  <panel layout="stretch 100%">
    <scrollable peeking="128" progress={<>ScrollProgress}>
      <grid margin="6,0,12,0" item-layout="count: 2" layout="stretch content">
        <frame *repeat={FilteredDisplayPaginated}
          layout="100% content"
          border-thickness="12"
          border={@mushymato.PerfectionHandbook/sprites/cursors:shopBg}>
          <lane orientation="Vertical">
            <lane padding="4,4,4,0" orientation="Horizontal" layout="content content" vertical-content-alignment="Middle">
              <image sprite={:SkillIcon} margin="8" layout="64px 64px" />
              <label font="small" text={SkillCountDisplay} max-lines="1" shadow-alpha="0.5" margin="4,0" />
              <panel margin="2" layout="stretch stretch" horizontal-content-alignment="End" vertical-content-alignment="End">
                <lane *repeat={LevelSquares} orientation="Horizontal" horizontal-content-alignment="Start" vertical-content-alignment="Middle">
                  <frame *repeat={:this} layout="24px 24px" margin="2" border-thickness="4" border={@Mods/StardewUI/Sprites/MenuSlotTransparent}>
                    <image *if={:Show}
                      sprite={@Mods/StardewUI/Sprites/White}
                      horizontal-alignment="Start"
                      tint={:Tint}
                      layout="100% stretch"
                      fit="Stretch"
                    />
                  </frame>
                </lane>
              </panel>
            </lane>
            <frame layout="100% 28px" margin="8,4,8,8" border-thickness="4" border={@Mods/StardewUI/Sprites/MenuSlotTransparent} focusable="true" screen-read={SkillCountDisplay}>
              <panel layout="100% stretch" vertical-content-alignment="End">
                <image
                  sprite={@Mods/StardewUI/Sprites/White}
                  horizontal-alignment="Start"
                  tint={ExpToNextTint}
                  layout={ExpToNextLayout}
                  fit="Stretch"
                />
                <label text={ExpToNextDisplay} horizontal-alignment="End" layout="stretch content" margin="2" shadow-alpha="1" />
              </panel>
            </frame>
          </lane>
        </frame>
      </grid>
    </scrollable>
  </panel>
</lane>
