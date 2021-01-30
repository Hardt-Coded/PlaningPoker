module Styling

    open Feliz
    open Feliz.MaterialUI
    open Fable.MaterialUI.Icons
    open Fable.MaterialUI.MaterialDesignIcons

    let useStyles = Styles.makeStyles(fun (styles:StyleCreator<unit>) theme ->
        let drawerWidth = 240
        {|
            root = styles.create [
                style.display.flex
            ]
            appBar = styles.create [
                style.zIndex (theme.zIndex.drawer + 1)
            ]
            appBarTitle = styles.create [
                style.flexGrow 1
            ]
            drawer = styles.create [
                style.width (length.px drawerWidth)
                style.flexShrink 0  // TODO: Does this do anything?
            ]
            drawerPaper = styles.create [
                style.width (length.px drawerWidth)
            ]
            content = styles.create [
                style.width 0  // TODO: is there a better way to prevent long code boxes extending past the screen?
                style.flexGrow 1
                style.padding (theme.spacing 3)
            ]
            nestedMenuItem = styles.create [
                style.paddingLeft (theme.spacing 4)
            ]
            toolbar = styles.create [
                yield! theme.mixins.toolbar
            ]
        |}
    )


    module Theme =

        let defaultTheme = Styles.createMuiTheme()

        let light = Styles.createMuiTheme([
            theme.palette.type'.light
            theme.palette.primary Colors.indigo
            theme.palette.secondary Colors.pink
            theme.palette.background.default' "#fff"
            theme.typography.h1.fontSize "3rem"
            theme.typography.h2.fontSize "2rem"
            theme.typography.h3.fontSize "1.5rem"
        ])

        let dark = Styles.createMuiTheme([
            theme.palette.type'.dark
            theme.palette.primary Colors.lightBlue
            theme.palette.secondary Colors.pink
            theme.palette.background.default' defaultTheme.palette.grey.``900``
            theme.typography.h1.fontSize "3rem"
            theme.typography.h2.fontSize "2rem"
            theme.typography.h3.fontSize "1.5rem"

            theme.overrides.muiAppBar.colorDefault [
                style.backgroundColor defaultTheme.palette.grey.A400
            ]
            theme.overrides.muiPaper.root [
                style.backgroundColor defaultTheme.palette.grey.A400
            ]
            theme.overrides.muiDrawer.paper [
                style.backgroundColor defaultTheme.palette.grey.``900``
            ]

            theme.props.muiAppBar [
                appBar.color.default'
            ]
        ])