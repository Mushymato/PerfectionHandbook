<panel *switch={InEventPage} layout="stretch 100%">
  <!-- Scroll -->
  <scrollable *case="false" peeking="128" scrollbar-margin="-18,0,0,0" progress={<>ScrollProgress}>
    <grid margin="4,0,12,0" item-layout="length: 400+" item-spacing="-4,-4" layout="stretch content"
        primary-item-count={>PrimaryItemCount}
        button-press=|HandleShoulderButtons($Button)|>
      <frame *repeat={:FilteredDisplayPaginated}
        layout="content content"
        padding="12"
        focusable="true"
        background={@Mods/StardewUI/Sprites/ShopEntryBorder}
        screen-read={ScreenRead}
        hovered-subject={:NpcInfo.Chara}
        left-click=|^HandleLeftClick(this)|>
          <panel vertical-content-alignment="End">
            <lane orientation="Horizontal" vertical-content-alignment="Middle">
              <image layout="64px 80px"
                fit="Contain"
                horizontal-alignment="middle"
                vertical-alignment="end"
                sprite={:MugShotSprite}
                tint={:DisplayTint}/>
              <lane orientation="Vertical" margin="0,0,12,0" >
                <label margin="8,0,0,8" layout="stretch content" font="small" text={:DisplayName} max-lines="-1" shadow-alpha="0.8"/>
                <frame layout="stretch 24px" border-thickness="4" border={@Mods/StardewUI/Sprites/MenuSlotTransparent}>
                  <panel layout="100% stretch" vertical-content-alignment="End">
                    <image sprite={@mushymato.PerfectionHandbook/sprites/cursors:heartFillPx} fit="Stretch" layout={FriendshipFillLayout}/>
                    <image sprite={@mushymato.PerfectionHandbook/sprites/cursors:heartFill} margin="-14,0,0,0" layout="28px 24px"/>
                    <label text={HeartLevel} margin="16,0,0,0" horizontal-alignment="Start" shadow-alpha="0.8" />
                    <label text={FriendshipPointDisplay} horizontal-alignment="End" layout="stretch content" shadow-alpha="0.8" />
                  </panel>
                </frame>
              </lane>
            </lane>
            <image *if={Reminder.Active} sprite={@mushymato.PerfectionHandbook/sprites/cursors:blueExclaim} layout="12px 32px" margin="4,0,0,44" />
          </panel>
      </frame>
    </grid>
  </scrollable>
  <!-- Events -->
  <lane *case="true" *context={Selected} margin="4,0,4,0" orientation="vertical">
    <frame padding="12" background={@Mods/StardewUI/Sprites/ShopEntryBorder}>
      <panel vertical-content-alignment="End">
        <lane orientation="Horizontal" vertical-content-alignment="Middle">
          <image layout="64px 80px"
            margin="8,8,0,0"
            fit="Contain"
            horizontal-alignment="middle"
            vertical-alignment="end"
            sprite={:MugShotSprite}
            tint={:DisplayTint}/>
          <lane orientation="Vertical" margin="0,0,12,0" >
            <label focusable="true"  margin="8,0,0,8" font="dialogue" text={:DisplayName} max-lines="-1" shadow-alpha="0.8"/>
            <lane vertical-content-alignment="Middle">
              <label focusable="true" margin="8,0" font="small" text={:NpcInfo.BirthdayText} max-lines="-1" shadow-alpha="0.8" />
              <frame layout="stretch 24px" margin="8,0,0,0" border-thickness="4" border={@Mods/StardewUI/Sprites/MenuSlotTransparent}>
                <panel layout="stretch stretch" vertical-content-alignment="End">
                  <image sprite={@mushymato.PerfectionHandbook/sprites/cursors:heartFillPx} fit="Stretch" layout={FriendshipFillLayout}/>
                  <image sprite={@mushymato.PerfectionHandbook/sprites/cursors:heartFill} margin="-14,0,0,0" layout="28px 24px"/>
                  <label text={HeartLevel} margin="16,0,0,0" horizontal-alignment="Start" shadow-alpha="0.8" />
                  <label text={FriendshipPointDisplay} horizontal-alignment="End" layout="stretch content" shadow-alpha="0.8" />
                </panel>
              </frame>
              <label *if={:NpcInfo.HasModName} focusable="true" margin="8,0,0,0" font="small" text={:NpcInfo.ModName} color={:NpcInfo.ModNameTint} max-lines="-1" shadow-alpha="0.8"/>
            </lane>
          </lane>
        </lane>
        <image *if={Reminder.Active} sprite={@mushymato.PerfectionHandbook/sprites/cursors:blueExclaim} layout="12px 32px" margin="4,0,0,44" />
      </panel>
    </frame>
    <scrollable peeking="128" scrollbar-margin="-18,0,0,0" >
      <lane orientation="vertical" layout="stretch content" margin="8">
        <lane *repeat={:EventDisplaysFiltered} *switch={HasSeen} orientation="horizontal">
          <image *case="true" layout="27px 27px" sprite={@mushymato.PerfectionHandbook/sprites/cursors_1_6:checkmark} />
          <spacer *case="false" layout="27px 27px" />
          <expander layout="stretch content" margin="0,0,0,4" is-expanded={<>IsExpanded}>
            <lane *outlet="header" margin="-14,0,8,0">
              <panel *if={:HasRequiredFriendshipForNPC} margin="0,0,8,0" horizontal-content-alignment="End" vertical-content-alignment="Middle">
                <image margin="0,0,32,0" sprite={@mushymato.PerfectionHandbook/sprites/cursors:heartFill} layout="28px 24px"/>
                <digits scale="3" number={RequiredHeartLevelForNPC} />
              </panel>
              <label text={:Info.HeaderText} layout="stretch content" shadow-alpha="0.8"/>
            </lane>
            <frame *if={IsExpanded} background={@Mods/StardewUI/Sprites/MenuSlotTransparent} padding="8" margin="48,0,0,0">
              <lane layout="stretch content" orientation="vertical">
                <label *if={:Info.HasModName} focusable="true" text={:Info.ModName} color={:Info.ModNameTint} shadow-alpha="0.8"/>
                <lane *repeat={:Preconds} *switch={:Status} padding="4">
                  <image *case="true" layout="27px 27px" sprite={@mushymato.PerfectionHandbook/sprites/cursors_1_6:checkmark} />
                  <spacer *case="false" layout="27px 27px" />
                  <label focusable="true" text={:Info.DisplayText} margin="8,0,0,0" shadow-alpha="0.8" />
                </lane>
              </lane>
            </frame>
          </expander>
        </lane>
      </lane>
    </scrollable>
  </lane>
</panel>
