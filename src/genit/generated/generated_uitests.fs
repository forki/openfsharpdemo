module generated_uitests

open generated_forms
open generated_validation
open canopy
open canopyExtensions
open page_register
open generated_fake_data
open helper_tests

let run () =

  let baseuri = "http://localhost:8083/"
  start chrome

  context "Register"

  before (fun _ -> goto page_register.uri)

  "New registration validation" &&& fun _ ->
    click "Submit"
    displayed "First Name is required"
    displayed "Last Name is required"
    displayed "Email is not a valid email"
    displayed "Email is required"
    displayed "Password must be between 6 and 100 characters"
    displayed "Password is required"
    displayed "Confirm Password must be between 6 and 100 characters"
    displayed "Confirm Password is required"

  "Name invalid" &&& fun _ ->
    name 65 Invalid
    name 66 Invalid

  "Name valid" &&& fun _ ->
    name 1 Valid
    name 2 Valid
    name 63 Valid
    name 64 Valid

  "Email invalid" &&& fun _ ->
    _email << "junk"
    click _submit

    displayed _emailNotValid

  "Email valid" &&& fun _ ->
    click _submit
    displayed _emailNotValid

    _email << "junk@null.dev"
    click _submit
    notDisplayed _emailNotValid

  "Password invalid" &&& fun _ ->
    password 1 Invalid
    password 2 Invalid
    password 3 Invalid
    password 4 Invalid
    password 5 Invalid
    password 101 Invalid

  "Password valid" &&& fun _ ->
    password 6 Valid
    password 7 Valid
    password 99 Valid
    password 100 Valid

  "Password mismatch" &&& fun _ ->
    _password << "123456"
    _confirm << "654321"
    click _submit
    displayed _passwordsMatch

  "Can register new unique user" &&& fun _ ->
    let firstName, lastName, email = generateUniqueUser ()

    _first_name << firstName
    _last_name << lastName
    _email << email
    _password << "test1234"
    _confirm << "test1234"
    click _submit

    on baseuri







  context "Scenario"

  "Register -> Search -> Add to Cart -> Checkout" &&& fun _ ->
    let firstName, lastName, email = generateUniqueUser ()
    let product = fake_product()

    addProduct product |> ignore
    register firstName lastName email

    //Search
    url "http://localhost:8083/product/search"
    "[name='Value']" << product.Name
    click "Submit"

    //View first result
    click "tbody tr:first"

    click "Add to Cart"

    //displayed "Product Name"

    click "Checkout"

    displayed "Thanks!"

    //count ".cart-item" 0
