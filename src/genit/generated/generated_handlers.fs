module generated_handlers

open dsl
open System.Web
open Suave
open Suave.Authentication
open Suave.State.CookieStateStore
open Suave.Filters
open Suave.Successful
open Suave.Redirection
open Suave.Operators
open generated_views
open generated_forms
open generated_types
open generated_validation
open generated_data_access
open generated_fake_data
open generated_bundles
open helper_html
open helper_handler
open forms
open Newtonsoft.Json
open Suave.RequestErrors

let hasHeader header (req : HttpRequest) = req.headers |> List.exists (fun header' -> header = header')
let fromJson<'a> json = JsonConvert.DeserializeObject(json, typeof<'a>) :?> 'a
let toJson obj = JsonConvert.SerializeObject(obj)
let mapErrors validation = validation |> List.map (fun (_,error) -> error)

let home = GET >=> OK view_jumbo_home

let thanks =
  GET >=> OK view_thanks

let register =
  choose
    [
      GET >=> OK view_register
      POST >=> bindToForm registerForm (fun registerForm ->
        let validation = validation_registerForm registerForm
        if validation = [] then
          let converted = convert_registerForm registerForm
          let id = insert_register converted
          setAuthCookieAndRedirect id "/"
        else
          OK (view_errored_register validation registerForm))
    ]

let login =
  choose
    [
      GET >=> (OK <| view_login false "")
      POST >=> request (fun req ->
        bindToForm loginForm (fun loginForm ->
        let validation = validation_loginForm loginForm
        if validation = [] then
          let converted = convert_loginForm loginForm
          let loginAttempt = authenticate converted
          match loginAttempt with
            | Some(_) ->
              let returnPath = getQueryStringValue req "returnPath"
              let returnPath = if returnPath = "" then "/" else returnPath
              setAuthCookieAndRedirect id returnPath
            | None -> OK <| view_login true loginForm.Email
        else
          OK (view_errored_login validation loginForm)))
    ]

let view_product id =
  GET >=> warbler (fun _ -> viewGET id bundle_product)

let search_product =
  choose
    [
      GET >=> request (fun req -> searchGET req bundle_product)
      POST >=> bindToForm searchForm (fun searchForm -> searchPOST searchForm bundle_product)
    ]

let view_cart id =
  GET >=> warbler (fun _ -> viewGET id bundle_cart)

let checkout =
  POST >=> bindToForm checkoutForm (fun form ->
    let checkout = { CheckoutID = 0L; CartFK = int64 form.CartFK }
    insert_checkout checkout |> ignore
    delete_cartItems checkout.CartFK
    FOUND "/thanks")

let add_to_cart =
  POST >=> bindToForm addToCartForm (fun addToCartForm ->
    getSession (fun session ->
      match session with
      | User(userID) ->
        //add a new cart every time, obviously wrong, demoware
        let cart = { CartID = 0L; UserFK = userID; Items = [] }
        let cartId = insert_cart cart
        let cartItem = { CartItemID = 0L; CartFK = cartId; ProductFK = int64 addToCartForm.ProductID }
        insert_cartItem cartItem |> ignore
        FOUND (sprintf "/cart/view/%i" cartId)
      | _ -> UNAUTHORIZED "Not logged in"))

//////API

let api_register =
  POST >=> request (fun req ->
    let register = fromJson<Register> (System.Text.Encoding.UTF8.GetString(req.rawForm))
    let validation = validation_registerJson register
    if validation = [] then
      let id = insert_register register
      OK ({ Data = id; Errors = [] } |> toJson)
    else
      let result = { Data = 0; Errors = mapErrors validation } |> toJson
      BAD_REQUEST result)

let api_product id =
  GET >=>
    let data = tryById_product id
    match data with
    | None -> NOT_FOUND error_404
    | Some(data) ->
       Writers.setMimeType "application/json"
       >=> OK (toJson { Data = data; Errors = [] })

let api_search_product =
  GET >=> request (fun req ->
      match req.queryParam "term" with
      | Choice1Of2 term -> OK (toJson { Data = generated_data_access.search_product term; Errors = [] })
      | Choice2Of2 _ -> BAD_REQUEST (toJson { Data = []; Errors = ["No search term provided"] }))

let api_create_product =
  POST >=> request (fun req ->
    let product = fromJson<Product> (System.Text.Encoding.UTF8.GetString(req.rawForm))
    let validation = validation_productJson product
    if validation = [] then
      let id = insert_product product
      OK (toJson { Data = id; Errors = [] })
    else
      let result = { Data = 0; Errors = mapErrors validation } |> toJson
      BAD_REQUEST result)

let api_edit_product =
  PUT >=> request (fun req ->
    let product = fromJson<Product> (System.Text.Encoding.UTF8.GetString(req.rawForm))
    let data = tryById_product product.ProductID
    match data with
    | None -> NOT_FOUND error_404
    | Some(_) ->
      let validation = validation_productJson product
      if validation = [] then
        update_product product
        NO_CONTENT
      else
        let result = { Data = 0; Errors = mapErrors validation } |> toJson
        BAD_REQUEST result)

let api_delete_product id =
  DELETE >=>
    let data = tryById_product id
    match data with
    | None -> NOT_FOUND error_404
    | Some(_) ->
        delete_product id
        NO_CONTENT

let api_cart id =
  GET >=>
    let data = tryById_cart id
    match data with
    | None -> NOT_FOUND error_404
    | Some(data) ->
       let data = { data with Items = getMany_cartItem_byCartId data.CartID }
       Writers.setMimeType "application/json"
       >=> OK (toJson { Data = data; Errors = [] })

let api_create_cart =
  POST >=> request (fun req ->
    let cart = fromJson<Cart> (System.Text.Encoding.UTF8.GetString(req.rawForm))
    let validation = validation_cartJson cart
    if validation = [] then
      let id = insert_cart cart
      OK (toJson { Data = id; Errors = [] })
    else
      let result = { Data = 0; Errors = mapErrors validation } |> toJson
      BAD_REQUEST result)

let api_add_to_cart =
  POST >=> request (fun req ->
    let cartItem = fromJson<CartItem> (System.Text.Encoding.UTF8.GetString(req.rawForm))
    let validation = validation_cartItemJson cartItem
    if validation = [] then
      let id = insert_cartItem cartItem
      OK (toJson { Data = id; Errors = [] })
    else
      let result = { Data = 0; Errors = mapErrors validation } |> toJson
      BAD_REQUEST result)

let api_checkout =
  POST >=> request (fun req ->
    let checkout = fromJson<Checkout> (System.Text.Encoding.UTF8.GetString(req.rawForm))
    let validation = validation_checkoutJson checkout
    if validation = [] then
      let id = insert_checkout checkout
      delete_cartItems checkout.CartFK
      OK (toJson { Data = id; Errors = [] })
    else
      let result = { Data = 0; Errors = mapErrors validation } |> toJson
      BAD_REQUEST result)
