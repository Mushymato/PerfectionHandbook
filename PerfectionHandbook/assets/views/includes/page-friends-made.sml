<panel *switch={InEventPage} layout="stretch 100%">
  <!-- Scroll -->
  <scrollable *case="false" peeking="128" scrollbar-margin="-18,0,0,0" progress={<>ScrollProgress}>
    <grid margin="4,0,12,0" item-layout="length: 400+" item-spacing="-4,-4" layout="stretch content"
        primary-item-count={>PrimaryItemCount}>
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
  <!-- Friend Detail + Events -->
  <lane *case="true" *context={Selected} margin="4,0,4,0" orientation="vertical">
    <!-- Friend Detail -->
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
    <!-- Event List -->
    <scrollable *!if={HasCurrentEventInfo} peeking="128" scrollbar-margin="-18,0,0,0" >
      <lane orientation="vertical" layout="stretch content" margin="8"  focusable="true">
        <frame *repeat={:EventDisplaysFiltered}
          border={@Mods/StardewUI/Sprites/MenuSlotTransparent}
          border-tint="Transparent"
          +hover:border-tint="White"
          focusable="true"
          margin="4,0,12,0"
          padding="8"
          left-click=|^ShowEvent(this)|
          >
          <event-header />
        </frame>
      </lane>
    </scrollable>
    <scrollable *if={HasCurrentEventInfo} *context={CurrentEventInfo} peeking="128" scrollbar-margin="-18,0,0,0" >
    <!-- Event Detail -->
      <lane layout="stretch content" orientation="vertical" margin="48,8,16,12">
        <frame margin="-36,0,0,4" padding="8" border={@Mods/StardewUI/Sprites/MenuSlotTransparent} focusable="true" focusable-tag={:Info.EventId}>
          <event-header />
        </frame>
        <lane *repeat={:Preconds} *switch={:Info.IsEventLink} padding="4" >
          <image *if={:Status} focusable="true" screen-read={:Info.DisplayText} layout="27px 27px" sprite={@mushymato.PerfectionHandbook/sprites/cursors_1_6:checkmark} />
          <spacer *!if={:Status} focusable="true" screen-read={:Info.DisplayText} layout="27px 27px" />
          <label *case="false" text={:Info.DisplayText} margin="8,0,0,0" shadow-alpha="0.8" />
          <lane *case="true" margin="8,0,0,0">
            <label text={:Info.PrecondText} shadow-alpha="0.8" />
            <label *repeat={:Info.EventLinks}
              focusable="true"
              margin="8,0,0,0"
              color="Blue"
              +hover:color="CornflowerBlue"
              shadow-alpha="0.8"
              text={:this}
              left-click=|~FriendsMadeDisplay.ShowEventById(this)| />
          </lane>
        </lane>
      </lane>
    </scrollable>
  </lane>
</panel>

<template name="event-header">
  <lane orientation="horizontal" vertical-content-alignment="Middle" layout="stretch content">
    <image *case={HasSeen} layout="27px 27px" sprite={@mushymato.PerfectionHandbook/sprites/cursors_1_6:checkmark} />
    <spacer *case={HasSeen} layout="27px 27px" />
    <panel *if={:HasRequiredFriendshipForNPC} margin="8,0" horizontal-content-alignment="End" vertical-content-alignment="Middle">
      <image margin="0,0,32,0" sprite={@mushymato.PerfectionHandbook/sprites/cursors:heartFill} layout="28px 24px"/>
      <digits scale="3" number={:RequiredHeartLevelForNPC} />
    </panel>
    <lane *if={:Info.HasModName} orientation="vertical" >
      <label text={:Info.HeaderText} shadow-alpha="0.8"/>
      <label text={:Info.ModName} color={:Info.ModNameTint} shadow-alpha="0.8" max-lines="1"/>
    </lane>
    <label *!if={:Info.HasModName} text={:Info.HeaderText} shadow-alpha="0.8"/>
  </lane>
</template>
