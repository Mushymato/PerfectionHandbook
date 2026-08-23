<panel layout="stretch 100%">
  <scrollable peeking="128" scrollbar-margin="-18,0,0,0" progress={<>ScrollProgress} scroll-step="528">
    <grid margin="0,0,16,0" item-layout="count: 5" layout="stretch content"
        primary-item-count={>PrimaryItemCount}
        button-press=|HandleShoulderButtons($Button)|>
      <frame *repeat={:FilteredDisplayPaginated}
        border={@Mods/StardewUI/Sprites/ShopEntryBorder}
        padding="24" margin="6" focusable="true"
        screen-read={:DisplayName}
        left-click=|ToggleReminder()|>
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
      </frame>
    </grid>
  </scrollable>
</panel>
