module Styling

    open Feliz
    open Feliz.MaterialUI




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


    let useStyles () =
        {
            root = "poker-root"
            appBar = "poker-appBar"
            appBarTitle = "poker-appBarTitle"

            content = "poker-content"

            toolbar = "poker-toolbar"

            centerPaper = "poker-centerPaper"

            playerCard = "poker-playerCard"

            loginPaper = "poker-loginPaper"

            loginAvatar = "poker-loginAvatar"

            loginForm = "poker-loginForm"

            loginSubmit = "poker-loginSubmit"
        }


    let useStyles2 = Styles.makeStyles(fun (styles:StyleCreator<unit>) theme ->
        {
            root = styles.create [
                style.display.flex
                style.width (length.px 1600)
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
                style.padding (theme.spacing(3, 2))
                style.display.flex
                style.flexDirection.column
                style.justifyContent.center
            ]

            playerCard = styles.create [
                style.padding (theme.spacing(1))
                style.display.flex
                style.flexDirection.column
                style.justifyContent.center
                style.minHeight 220
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
        let a = 1
        let defaultTheme = Styles.createTheme()

        let light = Styles.createTheme([
            theme.palette.primary Colors.indigo
            theme.palette.secondary Colors.pink
            theme.palette.background.default' defaultTheme.palette.grey.``100``
            //theme.palette.background.default' "#fff"
            theme.typography.h1.fontSize "3rem"
            theme.typography.h2.fontSize "2rem"
            theme.typography.h3.fontSize "1.5rem"
        ])

        let dark = Styles.createTheme([
            theme.palette.primary Colors.lightBlue
            theme.palette.secondary Colors.pink
            theme.palette.background.default' defaultTheme.palette.grey.``900``
            theme.typography.h1.fontSize "3rem"
            theme.typography.h2.fontSize "2rem"
            theme.typography.h3.fontSize "1.5rem"
            theme.palette.text.primary defaultTheme.palette.common.white
            theme.palette.text.secondary defaultTheme.palette.grey.A400
            theme.palette.text.disabled defaultTheme.palette.grey.A700
            theme.palette.action.active defaultTheme.palette.grey.A400

            theme.styleOverrides.muiContainer.root [
            ]

            theme.styleOverrides.muiGrid.root [
            ]

            theme.styleOverrides.muiAppBar.colorDefault [
                 style.backgroundColor defaultTheme.palette.grey.``800``
                 style.color defaultTheme.palette.common.white
             ]
            theme.styleOverrides.muiPaper.root [
                 style.backgroundColor defaultTheme.palette.grey.``800``
            ]
            theme.styleOverrides.muiDrawer.paper [
                style.backgroundColor defaultTheme.palette.grey.``900``
            ]

            theme.defaultProps.muiAppBar [
                appBar.color.default'
            ]
        ])

