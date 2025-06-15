module Dialog

    open Feliz
    open Feliz.MaterialUI
    

    let AlertDialog (isOpen:bool) (onClose:unit->unit) (title:string) (text:string) =
        Html.div [
            Mui.dialog [
                dialog.open' isOpen
                dialog.children [
                    Mui.dialogTitle title
                    Mui.dialogContent [
                        Mui.dialogContentText text
                    ]
                    Mui.dialogActions [
                        Mui.button [
                            prop.onClick (fun _ -> onClose())
                            prop.text "OK"
                        ]
                    ]    
                ]
            ]
            
        ]

