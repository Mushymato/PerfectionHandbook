<lane layout="stretch stretch" orientation="Vertical">
  <lane orientation="vertical" layout="stretch 100%" margin="24,12">
    <banner margin="8" text={#ui.misc.mod-config} layout="content content"/>
    <form-row title={#config.label.show-handbook-key} tooltip={#config.tooltip.show-handbook-key}>
      <keybind-editor button-height="64"
          sprite-map={@Mods/StardewUI/SpriteMaps/Buttons:default-default-0.5}
          editable-type="MultipleKeybinds"
          add-button-text={#config.label.show-handbook-key.add}
          focusable="true"
          keybind-list={<>ShowHandbookKey} />
    </form-row>
    <form-row title={#config.label.item-per-page} tooltip={#config.tooltip.item-per-page}>
      <spin-box *context={:ItemPerPageSpinBox} />
    </form-row>
  </lane>
</lane>

<template name="form-row">
  <lane layout="content content"
        vertical-content-alignment="middle"
        margin="16">
    <label layout="300px content"
            font="dialogue"
            text={&title}
            tooltip={&tooltip}
            shadow-alpha="0.8"
            shadow-color="#4448"
            shadow-offset="-2, 2" />
    <outlet />
  </lane>
</template>

<template name="spin-box">
  <lane orientation="horizontal" vertical-content-alignment="middle" margin="4,0">
    <image sprite={@Mods/StardewUI/Sprites/CaretLeft} focusable="true"
      left-click=|Decrease()|
      +hover:scale="1.2"
      +transition:scale="100ms EaseOutCubic"/>
    <label wheel=|Wheel($Direction)| text={ValueLabel}
      font="dialogue"
      layout="content[128..] content"
      padding="2,0,2,0"
      focusable="true"
      horizontal-alignment="middle"
      shadow-alpha="0.8"
      shadow-color="#4448"
      shadow-offset="-2, 2"/>
    <image sprite={@Mods/StardewUI/Sprites/CaretRight} focusable="true"
      left-click=|Increase()|
      +hover:scale="1.2"
      +transition:scale="100ms EaseOutCubic"/>
  </lane>
</template>
