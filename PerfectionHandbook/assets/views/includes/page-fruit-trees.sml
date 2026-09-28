<panel layout="stretch 100%">
  <scrollable peeking="128" scrollbar-margin="-18,0,0,0" progress={<>ScrollProgress} scroll-step="528">
    <grid item-layout="length: 168+" layout="stretch content" primary-item-count={>PrimaryItemCount}>
      <panel *repeat={:FilteredDisplayPaginated} scroll-with-children="Vertical" margin="12" horizontal-content-alignment="Middle" vertical-content-alignment="End">
        <image layout="144px 240px" horizontal-alignment="middle" vertical-alignment="end" sprite={:Sprite} tint={:DisplayTint}/>
        <panel layout="stretch stretch">
          <image *repeat={:FruitDisplays}
            sprite={:Info.Datum}
            hovered-subject={:Info.ReprItem}
            margin={:Margin}
            layout="48px 48px"
            shadow-offset="-4,4"
            +transition:scale="100ms EaseInSine"
            horizontal-alignment="Middle"
          />
        </panel>
        <frame focusable="true" padding="8" border={@Mods/StardewUI/Sprites/MenuSlotInset} hovered-subject={:SaplingInfo.ReprItem} tooltip={:DisplayText}>
          <lane vertical-content-alignment="middle">
            <image sprite={:SaplingInfo.Datum}
              layout="48px 48px"
              shadow-offset="-4,4"
              +transition:scale="100ms EaseInSine"
              horizontal-alignment="Middle"
            />
            <label margin="8,0" horizontal-alignment="End" font="dialogue" text={:Count} shadow-alpha="0.8"/>
          </lane>
        </frame>
      </panel>
    </grid>
  </scrollable>
</panel>
