<panel layout="stretch 100%">
  <scrollable peeking="128" scrollbar-margin="-18,0,0,0" progress={<>ScrollProgress} scroll-step="528">
    <grid item-layout="count: 5" item-spacing="-4,-4" layout="stretch content"
        primary-item-count={>PrimaryItemCount}>
      <frame *repeat={:FilteredDisplayPaginated}
        border={@Mods/StardewUI/Sprites/ShopEntryBorder}
        padding="24" focusable="true"
        screen-read={:DisplayName}
        left-click=|ToggleReminder()|>
        <panel layout="stretch content">
          <!-- Building Sprite -->
          <lane layout="stretch content" orientation="vertical" horizontal-content-alignment="Middle">
            <image layout="content 384px" fit="None" horizontal-alignment="middle" vertical-alignment="end" sprite={:Sprite} tint={:DisplayTint}/>
            <panel horizontal-content-alignment="Middle">
              <image *if={:HasShadow} layout={:ShadowLayout} fit="None" sprite={@mushymato.PerfectionHandbook/sprites/cursors:buildingShadow} tint={:DisplayTint}/>
              <label horizontal-alignment="middle"  layout="128px content" font="dialogue" text={:DisplayName} shadow-alpha="0.8" max-lines="2"/>
              <panel *context={:Reminder} layout="stretch stretch" vertical-content-alignment="Middle">
                <image *if={Active} sprite={@mushymato.PerfectionHandbook/sprites/cursors:blueExclaim} layout="12px 32px" margin="-24,0,0,0" />
              </panel>
            </panel>
          </lane>
          <!-- Building Materials -->
          <lane *if={:Needed} layout="stretch content" orientation="vertical" horizontal-content-alignment="Start">
            <lane *repeat={:Materials} tooltip={:Tooltip} hovered-subject={:Info.ReprItem} vertical-content-alignment="Middle">
              <image sprite={:Info.Datum}
                layout="32px 32px"
                shadow-offset="-4,4"
                +transition:scale="100ms EaseInSine"
                horizontal-alignment="Middle"
              />
              <label margin="4,0" text={:DisplayText} shadow-alpha="0.8"/>
            </lane>
          </lane>
        </panel>
      </frame>
    </grid>
  </scrollable>
</panel>
