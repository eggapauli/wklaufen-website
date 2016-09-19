#if COMPILED
module WkLaufen.Bmf2017.Form
#endif

type SimpleInputData = { Name: string; Description: string }
type CheckboxInputItem = { Value: string; Description: string; Checked: bool }
type CheckboxInputData = { Name: string; Description: string option; Items: CheckboxInputItem list }
type TextAreaInputData = { Common: SimpleInputData; Rows: string; Cols: string }

type Input =
    | TextInput of SimpleInputData
    | NumberInputWithPrefixTitle of SimpleInputData
    | NumberInputWithPostfixTitle of SimpleInputData
    | CheckboxInput of CheckboxInputData
    | RadioboxInput of CheckboxInputData
    | TextAreaInput of TextAreaInputData
