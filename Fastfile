default_platform(:ios)

platform :ios do
  lane :pilot_upload do
    api_key = app_store_connect_api_key(
      key_id: ENV["APP_STORE_CONNECT_API_KEY_KEY_ID"],
      issuer_id: ENV["APP_STORE_CONNECT_API_KEY_ISSUER_ID"],
      key_content: ENV["APP_STORE_CONNECT_API_KEY_KEY"],
      is_key_content_base64: true
    )

    ipa_path = ENV.fetch("IPA_PATH", "export/Brew.ipa")

    pilot(
      api_key: api_key,
      ipa: ipa_path,
      skip_waiting_for_build_processing: true
    )
  end
end
