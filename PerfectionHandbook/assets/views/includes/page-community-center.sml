<lane layout="stretch stretch" orientation="vertical">
  <include *context={:this} name="mushymato.PerfectionHandbook/views/includes/goal-infobar" />
  <image sprite={@Mods/StardewUI/Sprites/ThinHorizontalDivider} layout="stretch content" margin="-6,4,0,0" fit="Stretch"/>
  <panel layout="stretch 100%">
    <scrollable peeking="128" scrollbar-margin="-18,0,0,0" progress={<>ScrollProgress}>
      <grid margin="0,0,8,0" item-layout="count: 2" layout="stretch content">
        <lane *repeat={FilteredDisplayPaginated} margin="4" vertical-content-alignment="Middle">
          <panel focusable="true" tooltip={:BundleName} horizontal-content-alignment="Middle" vertical-content-alignment="End">
            <image layout="160px 160px" sprite={@mushymato.PerfectionHandbook/sprites/JunimoNote:pictureFrame} />
            <image layout="128px 128px" margin="16" sprite={:BundleIcon}/>
            <banner background={@mushymato.PerfectionHandbook/sprites/JunimoNote:textFrame} background-border-thickness="12,2" text={:BundleCompletionText} />
          </panel>
          <grid margin="4,0,0,0" layout="content content[128..]" item-layout="length: 80" item-spacing="-4,-4">
            <frame *repeat={:BundleIngredients}
              layout="content content"
              border={:IngredientBorder}
              border-thickness="8"
              tooltip={:Info.ReprItem}
              hovered-subject={:Info.ReprItem}
              focusable="true">
              <panel horizontal-content-alignment="End" vertical-content-alignment="End">
                <image sprite={:Info.Datum}
                  shadow-alpha="0.35"
                  layout="64px 64px"
                  shadow-offset="-4,4"
                />
                <panel *if={:HasQualityStar} layout="stretch stretch" horizontal-content-alignment="start" vertical-content-alignment="end">
                  <image sprite={:QualityStar} layout="24px 24px"/>
                </panel>
                <digits *if={:HasCount} scale="3" number={:Count} />
              </panel>
            </frame>
          </grid>
        </lane>
      </grid>
    </scrollable>
  </panel>
</lane>
