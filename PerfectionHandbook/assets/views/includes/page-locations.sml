<panel *switch={HasSelected} layout="stretch 100%">
  <!-- Event List -->
  <scrollable *case="false" peeking="128" scrollbar-margin="-18,0,0,0" progress={<>ScrollProgress}>
    <grid margin="4,0,12,0" item-layout="length: 500+" item-spacing="-4,-4" layout="stretch content"
      primary-item-count={>PrimaryItemCount}>
      <frame *repeat={:FilteredDisplayPaginated}
        layout="stretch content"
        padding="24,24"
        focusable="true"
        screen-read={:ScreenRead}
        background={@Mods/StardewUI/Sprites/ShopEntryBorder}
        left-click=|^HandleLeftClick(this)|>
        <lane orientation="vertical">
          <label text={:DisplayName} shadow-alpha="0.8" max-lines="1"/>
          <label text={:EventCount} shadow-alpha="0.8" max-lines="-1"/>
        </lane>
      </frame>
    </grid>
  </scrollable>
  <!-- Event List -->
  <panel *case="true" *context={Selected}>
    <scrollable *!if={HasCurrentEventInfo} peeking="128" layout="stretch" scrollbar-margin="-18,0,0,0" >
      <grid item-layout="count:2" layout="stretch content" margin="8">
        <frame *repeat={:EventDisplaysFiltered}
          border={@Mods/StardewUI/Sprites/MenuSlotTransparent}
          border-tint="Transparent"
          +hover:border-tint="White"
          margin="4,0,12,0"
          padding="8"
          left-click=|~GoalLocationContext.ShowEvent(this)|
          >
          <event-header text={:EventHeaderText}/>
        </frame>
      </grid>
    </scrollable>
    <!-- Event Detail -->
    <scrollable *if={HasCurrentEventInfo} peeking="128" layout="stretch" scrollbar-margin="-18,0,0,0" >
      <lane *context={CurrentEventInfo} layout="stretch content" orientation="vertical" margin="48,8,16,12">
        <frame border={@Mods/StardewUI/Sprites/MenuSlotTransparent}
          margin="-36,0,0,0" padding="8">
          <lane layout="stretch content" orientation="horizontal" vertical-content-alignment="End"
            left-click=|ToggleHeaderText()|
            >
            <event-header text={EventHeaderTextToggled}/>
            <image *repeat={:ActorLinks}
              padding="4,0,0,-4"
              fit="Contain"
              focusable="true"
              vertical-alignment="End"
              sprite={:MugShotSprite}
              tooltip={:Label}
            />
          </lane>
        </frame>
        <lane *if={:HasEventDescription} margin="-32,12,0,12" vertical-content-alignment="Middle">
          <image layout="28px 24px" margin="8,0" sprite={@mushymato.PerfectionHandbook/sprites/cursors_1_6:speechBubble} />
          <label text={:EventDescription} shadow-alpha="0.8" />
        </lane>
        <lane *repeat={:Preconds} *switch={:LinkKind} padding="4" >
          <image *if={:Status} focusable="true" screen-read={:Info.DisplayText} tooltip={:Info.Tooltip} layout="27px 27px" sprite={@mushymato.PerfectionHandbook/sprites/cursors_1_6:checkmark} />
          <spacer *!if={:Status} focusable="true" screen-read={:Info.DisplayText} tooltip={:Info.Tooltip} layout="27px 27px" />
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
              left-click=|~GoalLocationContext.ShowEventById(Link)| />
          </lane>
          <label *case="Friend" text={:Info.DisplayText} margin="8,0,0,0" shadow-alpha="0.8" />
          <!-- <lane *case="Friend" margin="8,0,0,0">
            <label text={:Info.PrecondText} shadow-alpha="0.8" />
            <label *repeat={:Links}
              focusable="true"
              margin="8,0,0,0"
              color={:TextColor}
              +hover:color={:TextHoverColor}
              shadow-alpha="0.8"
              text={:Label}
            />
          </lane> -->
        </lane>
      </lane>
    </scrollable>
  </panel>
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
      <label text={&text} shadow-alpha="0.8" max-lines="1"/>
      <label text={:Info.ModName} color={:Info.ModNameTint} shadow-alpha="0.8" max-lines="1"/>
    </lane>
    <label *!if={:Info.HasModName} text={&text}
      focusable="true"
      focusable-tag="default-focus"
      shadow-alpha="0.8"/>
  </lane>
</template>
