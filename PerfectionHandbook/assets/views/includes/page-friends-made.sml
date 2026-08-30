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
                <frame *if={:NpcInfo.CanEventuallySocialize} layout="stretch 24px" border-thickness="4" border={@Mods/StardewUI/Sprites/MenuSlotTransparent}>
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
    <frame padding="12" background={@Mods/StardewUI/Sprites/ShopEntryBorder} layout="stretch content">
      <panel vertical-content-alignment="End">
        <lane orientation="Horizontal" vertical-content-alignment="Middle">
          <image layout="content content"
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
              <frame *if={:NpcInfo.CanEventuallySocialize} layout="stretch 24px" margin="8,0,0,0" border-thickness="4" border={@Mods/StardewUI/Sprites/MenuSlotTransparent}>
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
    <scrollable *!if={HasCurrentEventInfo} peeking="128" layout="stretch" scrollbar-margin="-18,0,0,0" >
      <grid item-layout="count:2" layout="stretch content" margin="8">
        <frame *repeat={:EventDisplaysFiltered}
          border={@Mods/StardewUI/Sprites/MenuSlotTransparent}
          border-tint="Transparent"
          +hover:border-tint="White"
          margin="4,0,12,0"
          padding="8"
          left-click=|~GoalFriendsMadeContext.ShowEvent(this)|
          >
          <event-header />
        </frame>
      </grid>
    </scrollable>
  <!-- Event Detail -->
    <scrollable *if={HasCurrentEventInfo} peeking="128" layout="stretch" scrollbar-margin="-18,0,0,0" >
      <lane *context={CurrentEventInfo} layout="stretch content" orientation="vertical" margin="48,8,16,12">
        <frame border={@Mods/StardewUI/Sprites/MenuSlotTransparent}
          margin="-36,0,0,0" padding="8">
          <lane layout="stretch content" orientation="horizontal" vertical-content-alignment="End">
            <event-header/>
            <image *repeat={:ActorLinks}
              padding="4,0,0,-4"
              fit="Contain"
              focusable="true"
              vertical-alignment="End"
              sprite={:MugShotSprite}
              tooltip={:Label}
              left-click=|~GoalFriendsMadeContext.ShowFriendById(Link)|
            />
          </lane>
        </frame>
        <lane *if={:HasEventDescription} margin="-32,12,0,12" vertical-content-alignment="Middle">
          <image layout="28px 24px" margin="8,0" sprite={@mushymato.PerfectionHandbook/sprites/cursors_1_6:speechBubble} />
          <label text={:EventDescription} shadow-alpha="0.8" />
        </lane>
        <lane *repeat={:Preconds} *switch={:LinkKind} padding="4" >
          <image *if={:Status} focusable="true" screen-read={:Info.DisplayText} layout="27px 27px" sprite={@mushymato.PerfectionHandbook/sprites/cursors_1_6:checkmark} />
          <spacer *!if={:Status} focusable="true" screen-read={:Info.DisplayText} layout="27px 27px" />
          <label *case="None" text={:Info.DisplayText} margin="8,0,0,0" shadow-alpha="0.8" />
          <lane *case="Event" margin="8,0,0,0">
            <label text={:Info.PrecondText} shadow-alpha="0.8" />
            <label *repeat={:Links}
              focusable="true"
              margin="8,0,0,0"
              color={:TextColor}
              +hover:color={:TextHoverColor}
              shadow-alpha="0.8"
              text={:Label}
              left-click=|~GoalFriendsMadeContext.ShowEventById(Link)| />
          </lane>
          <lane *case="Friend" margin="8,0,0,0">
            <label text={:Info.PrecondText} shadow-alpha="0.8" />
            <label *repeat={:Links}
              focusable="true"
              margin="8,0,0,0"
              color={:TextColor}
              +hover:color={:TextHoverColor}
              shadow-alpha="0.8"
              text={:Label}
              left-click=|~GoalFriendsMadeContext.ShowFriendById(Link)| />
          </lane>
        </lane>
      </lane>
    </scrollable>
  </lane>
</panel>

<template name="event-header">
  <lane orientation="horizontal"
    vertical-content-alignment="Middle"
    layout="stretch content"
    screen-read={:Info.HeaderText}>
    <image *if={:HasSeen} margin="4,0" layout="27px 27px" sprite={@mushymato.PerfectionHandbook/sprites/cursors_1_6:checkmark} />
    <spacer *!if={:HasSeen} margin="4,0" layout="27px 27px" />
    <digits *if={:HasRequiredFriendshipForNPC} margin="0,0,4,0" scale="3" number={:RequiredHeartLevelForNPC} />
    <image *if={:HasRequiredFriendshipForNPC} sprite={@mushymato.PerfectionHandbook/sprites/cursors:heartFill} layout="28px 24px"/>
    <lane *if={:Info.HasModName} 
      focusable="true"
      focusable-tag="default-focus"
      margin="8,0"
      orientation="vertical">
      <label text={:EventHeaderText} shadow-alpha="0.8" max-lines="1"/>
      <label text={:Info.ModName} color={:Info.ModNameTint} shadow-alpha="0.8" max-lines="1"/>
    </lane>
    <label *!if={:Info.HasModName} text={:EventHeaderText}
      focusable="true"
      focusable-tag="default-focus"
      shadow-alpha="0.8"/>
  </lane>
</template>
