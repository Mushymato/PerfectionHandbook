<panel layout="stretch 100%">
  <scrollable peeking="128" scrollbar-margin="-18,0,0,0" progress={<>ScrollProgress}>
    <grid item-layout="count: 3" layout="stretch content" item-spacing="-4,-4"
        primary-item-count={>PrimaryItemCount}
        button-press=|HandleShoulderButtons($Button)|>
      <frame *repeat={FilteredDisplayPaginated}
        layout="content content"
        padding="12"
        focusable="true"
        tooltip={:TooltipText}
        background={@Mods/StardewUI/Sprites/ShopEntryBorder}
        left-click=|ToggleReminder()|>
        <panel>
          <image sprite={@Mods/StardewUI/Sprites/White} tint="#4CAF50" fit="Stretch" layout={QuestFillLayout}/>
          <lane padding="6" orientation="Horizontal" vertical-content-alignment="Middle">
            <image sprite={:DisplaySprite} fit="Contain" layout="32px 64px" horizontal-alignment="Middle" vertical-alignment="Middle"/>
            <label font="dialogue" text={:DisplayName} max-lines="1" shadow-alpha="0.5" layout="stretch content" />
            <label font="dialogue" text={:DisplayCounts} max-lines="1" shadow-alpha="0.5" />
          </lane>
        </panel>
      </frame>
    </grid>
  </scrollable>
</panel>
