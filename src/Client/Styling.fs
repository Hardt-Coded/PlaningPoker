module Styling

    open Feliz
    open Feliz.MaterialUI
    open Fable.MaterialUI.Icons
    open Fable.MaterialUI.MaterialDesignIcons


    type CustomStyles = {
        root: string
        appBar: string
        appBarTitle: string
        content: string
        toolbar: string
        centerPaper: string
        playerCard: string
        loginPaper: string
        loginAvatar: string
        loginForm: string
        loginSubmit: string
    }

    let useStyles = Styles.makeStyles(fun (styles:StyleCreator<unit>) theme ->
        {
            root = styles.create [
                style.display.flex
            ]
            appBar = styles.create [
                style.zIndex (theme.zIndex.drawer + 1)
            ]
            appBarTitle = styles.create [
                style.flexGrow 1
            ]
        
            content = styles.create [
                style.width 0  // TODO: is there a better way to prevent long code boxes extending past the screen?
                style.flexGrow 1
                style.padding (theme.spacing 3)
            ]
        
            toolbar = styles.create [
                yield! theme.mixins.toolbar
            ]

            centerPaper = styles.create [
                style.textAlign.center
                style.paddingTop 2
                style.paddingBottom 2
            ]

            playerCard = styles.create [
                style.textAlign.center
                style.padding 4
            ]

            loginPaper = styles.create [
                style.marginTop (theme.spacing 8)
                style.display.flex
                style.flexDirection.column
                style.alignItems.center
                style.padding 20
              ]

            loginAvatar = styles.create [
                style.margin (theme.spacing 1)
                style.backgroundColor theme.palette.primary.main
              ]

            loginForm = styles.create [
                style.width (length.perc 100)  // Allegedly fixes an IE 11 issue
                style.marginTop (theme.spacing 1)
              ]

            loginSubmit = styles.create [
                style.margin (theme.spacing(3, 0, 2))
              ]
        }
    )


    module Theme =

        let defaultTheme = Styles.createMuiTheme()

        let light = Styles.createMuiTheme([
            theme.palette.type'.light
            theme.palette.primary Colors.indigo
            theme.palette.secondary Colors.pink
            theme.palette.background.default' defaultTheme.palette.grey.``100``
            //theme.palette.background.default' "#fff"
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

        let a = 1