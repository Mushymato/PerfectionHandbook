<panel layout="stretch 100%">
  <scrollable peeking="128" scrollbar-margin="-18,0,0,0" progress={<>ScrollProgress} scroll-step="528">
    <grid item-layout="length: 216+" layout="stretch content" primary-item-count={>PrimaryItemCount}>
      <panel *repeat={:FilteredDisplayPaginated} scroll-with-children="Vertical" padding="12">
        <image layout="192px 320px" fit="None" horizontal-alignment="middle" vertical-alignment="end" sprite={:Sprite} />
        <panel layout="stretch stretch" vertical-content-alignment="End">
          <frame focusable="true" padding="8" border={@Mods/StardewUI/Sprites/MenuSlotInset} hovered-subject={:SaplingInfo.ReprItem} tooltip={:DisplayText}>
            <image sprite={:SaplingInfo.Datum}
              layout="64px 64px"
              shadow-offset="-4,4"
              +transition:scale="100ms EaseInSine"
              horizontal-alignment="Middle"
            />
          </frame>
        </panel>
        <image *repeat={:FruitDisplays}
          sprite={:Info.Datum}
          hovered-subject={:Info.ReprItem}
          margin={:Margin}
          layout="64px 64px"
          shadow-offset="-4,4"
          +transition:scale="100ms EaseInSine"
          horizontal-alignment="Middle"
        />
      </panel>
    </grid>
  </scrollable>
</panel>
