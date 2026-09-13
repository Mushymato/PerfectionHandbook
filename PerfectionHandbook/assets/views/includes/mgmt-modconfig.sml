<panel layout="stretch stretch">
  <banner background={@Mods/StardewUI/Sprites/BannerBackground}
    background-border-thickness="48,12"
    margin="12,-80,0,0" text={#ui.misc.mod-config} layout="content content" item-span="-1"/>
  <scrollable peeking="128" scrollbar-margin="-18,0,0,0">
    <grid item-layout="count: 10" layout="stretch content" margin="64,12,0,0">
      <!-- handbook -->
      <banner text={#config.section.Handbook} margin="-32,12,0,12" layout="content content" item-span="-1"/>
      <form-label text={#config.name.ShowHandbookKey} tooltip={#config.desc.ShowHandbookKey} />
      <form-cell>
        <keybind keybind-list={<>ShowHandbookKey} />
      </form-cell>

      <form-label text={#config.name.RowPerPage} tooltip={#config.desc.RowPerPage} />
      <form-cell>
        <spin-box *context={:RowPerPageSpinBox} />
      </form-cell>

      <form-label text={#config.name.AutoExportPeriod} tooltip={#config.desc.AutoExportPeriod} />
      <form-cell>
        <spin-box *context={:AutoExportPeriodSpinBox} />
      </form-cell>

      <form-label text={#config.name.CardWidth} tooltip={#config.desc.CardWidth} />
      <form-cell>
        <spin-box *context={:CardWidthSpinBox} />
      </form-cell>

      <form-label text={#config.name.StrictItemOwnedCheck} tooltip={#config.desc.StrictItemOwnedCheck} />
      <form-cell>
        <checkbox margin="4" is-checked={<>StrictItemOwnedCheck}/>
      </form-cell>

      <form-label *if={:HasModNamesAPI} text={#config.name.EnableModNames} tooltip={#config.desc.EnableModNames} />
      <form-cell *if={:HasModNamesAPI}>
        <checkbox margin="4" is-checked={<>EnableModNames}/>
      </form-cell>

      <spacer item-span="-1"/>

      <!-- reminders -->
      <banner text={#config.section.Reminders} margin="-32,12,0,12" layout="content content" item-span="-1"/>

      <form-label text={#config.name.RemindersToggleKey} tooltip={#config.desc.RemindersToggleKey} />
      <form-cell>
        <keybind keybind-list={<>RemindersToggleKey} />
      </form-cell>

      <form-label text={#config.name.RemindersEditModifierKey} tooltip={#config.desc.RemindersEditModifierKey} />
      <form-cell>
        <keybind keybind-list={<>RemindersEditModifierKey} />
      </form-cell>

      <form-label text={#config.name.RemindersMaxCount} tooltip={#config.desc.RemindersMaxCount} />
      <form-cell>
        <spin-box *context={:RemindersMaxCountSpinBox} />
      </form-cell>

      <form-label text={#config.name.RemindersDefaultExpanded} tooltip={#config.desc.RemindersDefaultExpanded} />
      <form-cell>
        <checkbox margin="4" is-checked={<>RemindersDefaultExpanded}/>
      </form-cell>

      <form-label text={#config.name.RemindersHUDPosition} tooltip={#config.desc.RemindersHUDPosition} />
      <form-cell>
        <nine-grid-editor layout="48px"
          hover-tint-color="orange"
          button-sprite-map={@Mods/StardewUI/SpriteMaps/Buttons:dark-light}
          direction-sprite-map={@Mods/StardewUI/SpriteMaps/Directions}
          placement={<>RemindersHUDPosition}
          focusable="true">
          <include *context={:RemindersHUDCtx} name="mushymato.PerfectionHandbook/views/reminder-hud" />
        </nine-grid-editor>
      </form-cell>
    </grid>
  </scrollable>
  <button *float="Below"
    hover-background={@Mods/StardewUI/Sprites/ButtonLight}
    font="dialogue"
    margin="10,-72,16,0"
    text={#ui.reset}
    left-click=|ResetConfigsToDefault()|
  />
</panel>

<template name="form-label">
  <panel item-span="2"
    layout="stretch 88px"
    margin="0,0,12,0"
    horizontal-content-alignment="Start"
    vertical-content-alignment="Middle"
    focusable="true"
    tooltip={&tooltip}>
    <label layout="content content"
      text={&text}
      shadow-alpha="0.8"
      shadow-offset="-2, 2"/>
  </panel>
</template>

<template name="form-cell">
  <panel item-span="3"
    layout="stretch 88px"
    margin="4,0,0,0"
    horizontal-content-alignment="Start"
    vertical-content-alignment="Middle">
    <outlet/>
  </panel>
</template>

<template name="keybind">
  <keybind-editor
    button-height="64"
    sprite-map={@Mods/StardewUI/SpriteMaps/Buttons:default-default-0.5}
    editable-type="MultipleKeybinds"
    add-button-text={#config.label.add-key}
    focusable="true"
    keybind-list={&keybind-list} />
</template>

<template name="spin-box">
  <lane layout="content 60px" orientation="horizontal" vertical-content-alignment="Middle" margin="4,0">
    <image sprite={@Mods/StardewUI/Sprites/CaretLeft} focusable="true"
      left-click=|Decrease()|
      +hover:scale="1.2"
      +transition:scale="100ms EaseOutCubic"/>
    <label wheel=|Wheel($Direction)|
      text={Value}
      font="dialogue"
      layout="content[80..] content"
      padding="2,0,2,0"
      focusable="true"
      horizontal-alignment="middle"
      shadow-alpha="0.8"
      shadow-offset="-2, 2"/>
    <image sprite={@Mods/StardewUI/Sprites/CaretRight} focusable="true"
      left-click=|Increase()|
      +hover:scale="1.2"
      +transition:scale="100ms EaseOutCubic"/>
  </lane>
</template>
